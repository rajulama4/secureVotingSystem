using SecureVoting.API.Models;
using System.Data;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class ApiLogRepository
    {
        private readonly DbHelper _db;
        public ApiLogRepository(DbHelper db) => _db = db;

        public void Insert(ApiLog log)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_ApiLog_Insert", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)log.UserId ?? DBNull.Value;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = (object?)log.Email ?? DBNull.Value;

            cmd.Parameters.Add("@Endpoint", SqlDbType.NVarChar, 500).Value = log.Endpoint ?? "";
            cmd.Parameters.Add("@HttpMethod", SqlDbType.NVarChar, 10).Value = log.HttpMethod ?? "";
            cmd.Parameters.Add("@RequestTimeUtc", SqlDbType.DateTime2).Value = log.RequestTimeUtc;

            cmd.Parameters.Add("@IpAddress", SqlDbType.NVarChar, 64).Value = (object?)log.IpAddress ?? DBNull.Value;
            cmd.Parameters.Add("@UserAgent", SqlDbType.NVarChar, 512).Value = (object?)log.UserAgent ?? DBNull.Value;

            cmd.Parameters.Add("@IsSuccess", SqlDbType.Bit).Value = log.IsSuccess;
            cmd.Parameters.Add("@StatusCode", SqlDbType.Int).Value = log.StatusCode;

          
            cmd.Parameters.Add("@ApiReq", SqlDbType.NVarChar, -1).Value = (object?)log.ApiReqMasked ?? DBNull.Value;
            cmd.Parameters.Add("@ApiRes", SqlDbType.NVarChar, -1).Value = (object?)log.ApiResMasked ?? DBNull.Value;

     
            cmd.Parameters.Add("@ApiReqEnc", SqlDbType.VarBinary, -1).Value = (object?)log.ApiReqEnc ?? DBNull.Value;
            cmd.Parameters.Add("@ApiResEnc", SqlDbType.VarBinary, -1).Value = (object?)log.ApiResEnc ?? DBNull.Value;

            //cmd.Parameters.Add("@DurationMs", SqlDbType.Int).Value = (object?)log.DurationMs ?? DBNull.Value;

            conn.Open();
            cmd.ExecuteNonQuery();
        }


        public (byte[]? reqEnc, byte[]? resEnc)? GetEncrypted(int logId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_ApiLog_GetEncrypted", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@LogId", SqlDbType.Int).Value = logId;

            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            byte[]? req = r.IsDBNull(r.GetOrdinal("API_Req_Enc")) ? null : (byte[])r["API_Req_Enc"];
            byte[]? res = r.IsDBNull(r.GetOrdinal("API_Res_Enc")) ? null : (byte[])r["API_Res_Enc"];
            return (req, res);
        }


        public List<ApiLog> GetLatest(
            int take = 50,
            int skip = 0,
            string? email = null,
            int? userId = null,
            string? endpoint = null)
        {
            var list = new List<ApiLog>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_ApiLog_GetLatestFiltered", conn)
            {
                CommandType = CommandType.StoredProcedure
            };

            cmd.Parameters.Add("@Take", SqlDbType.Int).Value = take;
            cmd.Parameters.Add("@Skip", SqlDbType.Int).Value = skip;
            cmd.Parameters.Add("@Email", SqlDbType.NVarChar, 150).Value = (object?)email ?? DBNull.Value;
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = (object?)userId ?? DBNull.Value;
            cmd.Parameters.Add("@Endpoint", SqlDbType.NVarChar, 200).Value = (object?)endpoint ?? DBNull.Value;

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                var log = new ApiLog
                {
                    LogId = r.GetInt32(r.GetOrdinal("LogId")),
                    UserId = r.IsDBNull(r.GetOrdinal("UserId")) ? null : r.GetInt32(r.GetOrdinal("UserId")),
                    Email = r.IsDBNull(r.GetOrdinal("Email")) ? null : r.GetString(r.GetOrdinal("Email")),
                    Endpoint = r.GetString(r.GetOrdinal("Endpoint")),
                    HttpMethod = r.GetString(r.GetOrdinal("HttpMethod")),
                    RequestTimeUtc = r.GetDateTime(r.GetOrdinal("RequestTimeUtc")),
                    IpAddress = r.IsDBNull(r.GetOrdinal("IpAddress")) ? null : r.GetString(r.GetOrdinal("IpAddress")),
                    UserAgent = r.IsDBNull(r.GetOrdinal("UserAgent")) ? null : r.GetString(r.GetOrdinal("UserAgent")),
                    IsSuccess = r.GetBoolean(r.GetOrdinal("IsSuccess")),
                    StatusCode = r.GetInt32(r.GetOrdinal("StatusCode")),
                    DurationMs = r.IsDBNull(r.GetOrdinal("DurationMs")) ? null : r.GetInt32(r.GetOrdinal("DurationMs")),

                 
                    ApiReqMasked = r.IsDBNull(r.GetOrdinal("API_Req")) ? null : r.GetString(r.GetOrdinal("API_Req")),
                    ApiResMasked = r.IsDBNull(r.GetOrdinal("API_Res")) ? null : r.GetString(r.GetOrdinal("API_Res")),
                };

                list.Add(log);
            }

            return list;
        }


        public ApiLog? GetById(int logId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand("dbo.sp_ApiLog_GetById", conn)
            {
                CommandType = CommandType.StoredProcedure
            };
            cmd.Parameters.Add("@LogId", SqlDbType.Int).Value = logId;

            conn.Open();
            using var r = cmd.ExecuteReader();
            if (!r.Read()) return null;

            return new ApiLog
            {
                LogId = r.GetInt32(r.GetOrdinal("LogId")),
                UserId = r.IsDBNull(r.GetOrdinal("UserId")) ? null : r.GetInt32(r.GetOrdinal("UserId")),
                Email = r.IsDBNull(r.GetOrdinal("Email")) ? null : r.GetString(r.GetOrdinal("Email")),
                Endpoint = r.GetString(r.GetOrdinal("Endpoint")),
                HttpMethod = r.GetString(r.GetOrdinal("HttpMethod")),
                RequestTimeUtc = r.GetDateTime(r.GetOrdinal("RequestTimeUtc")),
                IpAddress = r.IsDBNull(r.GetOrdinal("IpAddress")) ? null : r.GetString(r.GetOrdinal("IpAddress")),
                UserAgent = r.IsDBNull(r.GetOrdinal("UserAgent")) ? null : r.GetString(r.GetOrdinal("UserAgent")),
                IsSuccess = r.GetBoolean(r.GetOrdinal("IsSuccess")),
                StatusCode = r.GetInt32(r.GetOrdinal("StatusCode")),
                DurationMs = r.IsDBNull(r.GetOrdinal("DurationMs")) ? null : r.GetInt32(r.GetOrdinal("DurationMs")),

                ApiReqMasked = r.IsDBNull(r.GetOrdinal("API_Req")) ? null : r.GetString(r.GetOrdinal("API_Req")),
                ApiResMasked = r.IsDBNull(r.GetOrdinal("API_Res")) ? null : r.GetString(r.GetOrdinal("API_Res")),

                ApiReqEnc = r.IsDBNull(r.GetOrdinal("API_Req_Enc")) ? null : (byte[])r["API_Req_Enc"],
                ApiResEnc = r.IsDBNull(r.GetOrdinal("API_Res_Enc")) ? null : (byte[])r["API_Res_Enc"],
            };
        }
    }
}