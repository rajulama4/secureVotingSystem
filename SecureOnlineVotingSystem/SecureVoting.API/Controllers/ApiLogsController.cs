using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureVoting.API.Data;
using SecureVoting.API.Services;
using System.Data.SqlClient;
using System.Security.Claims;

[ApiController]
[Route("api/audit")]
public class AuditController : ControllerBase
{
    private readonly ApiLogRepository _logs;
    private readonly ApiLogCryptoService _crypto;
    private readonly DbHelper _db;

    public AuditController(ApiLogRepository logs, ApiLogCryptoService crypto, DbHelper db)
    {
        _logs = logs;
        _crypto = crypto;
        _db = db;
    }

    public record DecryptRequest(int LogId, string? Reason);

    [Authorize(Roles = "Admin")]
    [HttpPost("decrypt-log")]
    public IActionResult DecryptLog([FromBody] DecryptRequest req)
    {
        var enc = _logs.GetEncrypted(req.LogId);
        if (enc == null) return NotFound(new { message = "Log not found." });

        var reqJson = enc.Value.reqEnc == null ? null : _crypto.DecryptToString(enc.Value.reqEnc);
        var resJson = enc.Value.resEnc == null ? null : _crypto.DecryptToString(enc.Value.resEnc);

        // audit the access
        var userId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var uid) ? uid : (int?)null;
        var email = User.FindFirstValue(ClaimTypes.Email);
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();

        using var conn = _db.GetConnection();
        using var cmd = new SqlCommand("dbo.sp_ApiLogAccess_Insert", conn)
        {
            CommandType = System.Data.CommandType.StoredProcedure
        };
        cmd.Parameters.AddWithValue("@LogId", req.LogId);
        cmd.Parameters.AddWithValue("@AccessedByUserId", (object?)userId ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AccessedByEmail", (object?)email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@IpAddress", (object?)ip ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Reason", (object?)req.Reason ?? DBNull.Value);

        conn.Open();
        cmd.ExecuteNonQuery();

        return Ok(new { logId = req.LogId, apiReqFull = reqJson, apiResFull = resJson });
    }
}