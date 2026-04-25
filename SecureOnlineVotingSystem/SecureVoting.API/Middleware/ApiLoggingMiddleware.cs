using SecureVoting.API.Data;
using SecureVoting.API.Models;
using SecureVoting.API.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SecureVoting.API.Middleware
{
    public class ApiLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiLoggingMiddleware(RequestDelegate next) => _next = next;

        public async Task Invoke(HttpContext context, ApiLogRepository repo, ApiLogCryptoService crypto)
        {
            var startUtc = DateTime.UtcNow;

            // -------- Capture REQUEST body --------
            string reqBodyRaw = "";
            try
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(
                    context.Request.Body,
                    encoding: Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    bufferSize: 1024,
                    leaveOpen: true);

                reqBodyRaw = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }
            catch { /* ignore */ }

            // -------- Capture RESPONSE body --------
            var originalBody = context.Response.Body;
            await using var mem = new MemoryStream();
            context.Response.Body = mem;

            string resBodyRaw = "";

            try
            {
                await _next(context);
            }
            finally
            {
                // read response
                mem.Position = 0;
                try
                {
                    using var resReader = new StreamReader(mem, Encoding.UTF8, leaveOpen: true);
                    resBodyRaw = await resReader.ReadToEndAsync();
                }
                catch { /* ignore */ }

                // copy back to original stream so client still gets response
                mem.Position = 0;
                await mem.CopyToAsync(originalBody);
                context.Response.Body = originalBody;

                // -------- Determine UserId/Email --------
                int? userId = null;
                string? email = null;

                // 1) from claims (authenticated requests)
                var uidClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(uidClaim, out var parsedId)) userId = parsedId;
                email = context.User.FindFirst(ClaimTypes.Email)?.Value;

                // 2) from controller-set items (pre-auth endpoints)
                if (userId == null && context.Items.TryGetValue("LogUserId", out var idObj) && idObj is int idVal)
                    userId = idVal;

                if (string.IsNullOrWhiteSpace(email) && context.Items.TryGetValue("LogEmail", out var emailObj))
                    email = emailObj?.ToString();

                // 3) fallback: try parse from request JSON (login / verify-totp etc.)
                if ((userId == null || string.IsNullOrWhiteSpace(email)) && !string.IsNullOrWhiteSpace(reqBodyRaw))
                {
                    try
                    {
                        using var doc = JsonDocument.Parse(reqBodyRaw);
                        var root = doc.RootElement;

                        if (string.IsNullOrWhiteSpace(email) && root.TryGetProperty("email", out var emailProp))
                            email = emailProp.GetString();

                        if (userId == null && root.TryGetProperty("userId", out var userIdProp) && userIdProp.TryGetInt32(out var uid))
                            userId = uid;
                    }
                    catch { /* ignore */ }
                }

                // -------- Mask ONLY password for readable columns --------
                var reqBodyMasked = MaskPasswordOnly(reqBodyRaw);
                var resBodyMasked = MaskPasswordOnly(resBodyRaw); // usually no password in response, but safe

                // Optional: pretty JSON for readability
                reqBodyMasked = PrettyIfJson(reqBodyMasked);
                resBodyMasked = PrettyIfJson(resBodyMasked);

                // -------- Encrypt FULL raw request/response (Option 2) --------
                byte[]? reqEnc = null;
                byte[]? resEnc = null;

                try { reqEnc = crypto.EncryptString(PrettyIfJson(reqBodyRaw)); } catch { /* ignore */ }
                try { resEnc = crypto.EncryptString(PrettyIfJson(resBodyRaw)); } catch { /* ignore */ }

                // -------- Write log --------
                try
                {
                    repo.Insert(new ApiLog
                    {
                        UserId = userId,
                        Email = email,
                        Endpoint = context.Request.Path,
                        HttpMethod = context.Request.Method,
                        RequestTimeUtc = startUtc,
                        IpAddress = context.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = context.Request.Headers["User-Agent"].ToString(),
                        IsSuccess = context.Response.StatusCode >= 200 && context.Response.StatusCode < 300,
                        StatusCode = context.Response.StatusCode,
                        ApiReqMasked = reqBodyMasked,
                        ApiResMasked = resBodyMasked,
                        ApiReqEnc = reqEnc,
                        ApiResEnc = resEnc
                    });
                }
                catch { /* never break app */ }
            }
        }

        private static string MaskPasswordOnly(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return text;

            // masks only JSON "password": "..."
            return Regex.Replace(
                text,
                "(\"password\"\\s*:\\s*\")([^\"]*)(\")",
                "$1***$3",
                RegexOptions.IgnoreCase
            );
        }

        private static string PrettyIfJson(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            s = s.Trim();
            if (!(s.StartsWith("{") || s.StartsWith("["))) return s;

            try
            {
                using var doc = JsonDocument.Parse(s);
                return JsonSerializer.Serialize(doc, new JsonSerializerOptions { WriteIndented = true });
            }
            catch
            {
                return s;
            }
        }
    }
}