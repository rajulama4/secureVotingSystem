using SecureVoting.API.Models;
using System.Data.SqlClient;

namespace SecureVoting.API.Data
{
    public class JurisdictionRepository
    {
        private readonly DbHelper _db;
        public JurisdictionRepository(DbHelper db) => _db = db;

        public List<JurisdictionDto> GetAll()
        {
            var list = new List<JurisdictionDto>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT JurisdictionId, JurisdictionName, County, City, ZipCode
        FROM Jurisdictions
        ORDER BY JurisdictionName
    ", conn);

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add(new JurisdictionDto
                {
                    JurisdictionId = r.GetInt32(0),
                    Name = r.GetString(1),
                    County = r.GetString(2),
                    City = r.GetString(3),
                    ZipCode = r.GetString(4)
                });
            }

            return list;
        }
    }
}
