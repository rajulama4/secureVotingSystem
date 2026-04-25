namespace SecureVoting.API.Models
{
    public class ApiLog
    {
        public int? UserId { get; set; }
        public string? Email { get; set; }
        public string Endpoint { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public DateTime RequestTimeUtc { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public bool IsSuccess { get; set; }
        public int StatusCode { get; set; }


        public int? DurationMs { get; set; }

        // masked readable
        public string? ApiReqMasked { get; set; }   // maps to API_Req
        public string? ApiResMasked { get; set; }   // maps to API_Res

        // encrypted full
        public byte[]? ApiReqEnc { get; set; }      // maps to API_Req_Enc
        public byte[]? ApiResEnc { get; set; }      // maps to API_Res_Enc


        public int? LogId { get; set; }

    }
}
