using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Services;

namespace SecureVoting.API.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminLogsController : ControllerBase
    {
        [HttpGet("api-logs")]
        public IActionResult GetLogs(
            [FromServices] ApiLogRepository repo,
            [FromQuery] int take = 50,
            [FromQuery] int skip = 0,
            [FromQuery] string? email = null,
            [FromQuery] int? userId = null,
            [FromQuery] string? endpoint = null
        )
        {
            take = Math.Clamp(take, 1, 200);
            skip = Math.Max(0, skip);

            var logs = repo.GetLatest(take, skip, email, userId, endpoint);

            // NOTE: do NOT return payloads in list view (fast + safer)
            return Ok(logs.Select(l => new {
                l.LogId,
                l.UserId,
                l.Email,
                l.Endpoint,
                l.HttpMethod,
                l.RequestTimeUtc,
                l.IpAddress,
                l.UserAgent,
                l.IsSuccess,
                l.StatusCode,
                l.DurationMs
            }));
        }

        [HttpGet("api-logs/{logId:int}")]
        public IActionResult GetLogDetail(
            int logId,
            [FromServices] ApiLogRepository repo,
            [FromServices] ApiLogCryptoService crypto
        )
        {
            var log = repo.GetById(logId);
            if (log == null) return NotFound();

            // Prefer encrypted, fallback to readable (older rows)
            var req = log.ApiReqEnc != null ? crypto.DecryptToString(log.ApiReqEnc) : log.ApiReqMasked;
            var res = log.ApiResEnc != null ? crypto.DecryptToString(log.ApiResEnc) : log.ApiResMasked;

            return Ok(new
            {
                log.LogId,
                log.UserId,
                log.Email,
                log.Endpoint,
                log.HttpMethod,
                log.RequestTimeUtc,
                log.IpAddress,
                log.UserAgent,
                log.IsSuccess,
                log.StatusCode,
                log.DurationMs,
                apiReq = req,
                apiRes = res
            });
        }
    }
}