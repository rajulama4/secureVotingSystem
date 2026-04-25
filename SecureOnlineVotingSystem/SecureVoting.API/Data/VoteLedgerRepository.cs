using System.Data;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using SecureVoting.API.Services;
namespace SecureVoting.API.Data
{
    public class VoteLedgerRepository
    {

        private readonly DbHelper _db;

        public VoteLedgerRepository(DbHelper db) => _db = db;

        public byte[]? GetLastVoteHash(int electionId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                SELECT TOP 1 VoteHash
                FROM VoteLedger
                WHERE ElectionId = @ElectionId
                ORDER BY LedgerId DESC
            ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            var result = cmd.ExecuteScalar();
            return result == null ? null : (byte[])result;
        }

        public int InsertVote(int electionId, byte[] encryptedVote, Guid receiptId, byte[] voteHash, byte[]? previousHash)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
                INSERT INTO VoteLedger (ElectionId, EncryptedVote, ReceiptId, VoteHash, PreviousHash)
                VALUES (@ElectionId, @EncryptedVote, @ReceiptId, @VoteHash, @PreviousHash);
                SELECT SCOPE_IDENTITY();
            ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;
            cmd.Parameters.Add("@EncryptedVote", SqlDbType.VarBinary, -1).Value = encryptedVote;
            cmd.Parameters.Add("@ReceiptId", SqlDbType.UniqueIdentifier).Value = receiptId;
            cmd.Parameters.Add("@VoteHash", SqlDbType.VarBinary, 64).Value = voteHash;
            cmd.Parameters.Add("@PreviousHash", SqlDbType.VarBinary, 64).Value =
                (object?)previousHash ?? DBNull.Value;

            conn.Open();
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public static byte[] ComputeVoteHash(int electionId, Guid receiptId, byte[] encryptedVote, byte[]? previousHash)
        {
            // Hash of: electionId | receiptId | previousHash | encryptedVote
            using var sha = SHA256.Create();

            byte[] eBytes = BitConverter.GetBytes(electionId);
            byte[] rBytes = receiptId.ToByteArray();
            byte[] pBytes = previousHash ?? Array.Empty<byte>();

            byte[] combined = new byte[eBytes.Length + rBytes.Length + pBytes.Length + encryptedVote.Length];
            Buffer.BlockCopy(eBytes, 0, combined, 0, eBytes.Length);
            Buffer.BlockCopy(rBytes, 0, combined, eBytes.Length, rBytes.Length);
            Buffer.BlockCopy(pBytes, 0, combined, eBytes.Length + rBytes.Length, pBytes.Length);
            Buffer.BlockCopy(encryptedVote, 0, combined, eBytes.Length + rBytes.Length + pBytes.Length, encryptedVote.Length);

            return sha.ComputeHash(combined); // 32 bytes
        }


        public List<(Guid receiptId, byte[] encryptedVote)> GetVotesForElection(int electionId)
        {
            var list = new List<(Guid, byte[])>();

            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT ReceiptId, EncryptedVote
        FROM VoteLedger
        WHERE ElectionId = @ElectionId
        ORDER BY LedgerId
    ", conn);

            cmd.Parameters.Add("@ElectionId", SqlDbType.Int).Value = electionId;

            conn.Open();
            using var r = cmd.ExecuteReader();

            while (r.Read())
            {
                list.Add((
                    r.GetGuid(0),
                    (byte[])r[1]
                ));
            }

            return list;
        }

        public byte[]? GetEncryptedVoteByReceipt(Guid receiptId)
        {
            using var conn = _db.GetConnection();
            using var cmd = new SqlCommand(@"
        SELECT EncryptedVote
        FROM VoteLedger
        WHERE ReceiptId = @ReceiptId
    ", conn);

            cmd.Parameters.Add("@ReceiptId", SqlDbType.UniqueIdentifier).Value = receiptId;

            conn.Open();
            return cmd.ExecuteScalar() as byte[];
        }
    }
}
