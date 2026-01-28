using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SecureVoting.API.Services
{
    public class VoteCryptoService
    {
        private readonly byte[] _masterKey;

        public VoteCryptoService(IConfiguration config)
        {
            // Store in appsettings.json -> "VoteCrypto": { "MasterKeyBase64": "..." }
            var b64 = config["VoteCrypto:MasterKeyBase64"];
            if (string.IsNullOrWhiteSpace(b64))
                throw new InvalidOperationException("VoteCrypto:MasterKeyBase64 is missing from configuration.");

            _masterKey = Convert.FromBase64String(b64);
            if (_masterKey.Length != 32)
                throw new InvalidOperationException("MasterKey must be 32 bytes (base64 of 32-byte key) for AES-256.");
        }

        public byte[] EncryptVotePayload(object payload)
        {
            byte[] plaintext = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload));

            byte[] nonce = RandomNumberGenerator.GetBytes(12);      // recommended nonce size for GCM
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] tag = new byte[16];

            using var aes = new AesGcm(_masterKey);
            aes.Encrypt(nonce, plaintext, ciphertext, tag);

            // Store as: [nonce|tag|ciphertext]
            byte[] packed = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, packed, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, packed, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, packed, nonce.Length + tag.Length, ciphertext.Length);

            return packed;
        }

        public string DecryptToJson(byte[] packed)
        {
            byte[] nonce = packed[..12];
            byte[] tag = packed[12..28];
            byte[] ciphertext = packed[28..];

            byte[] plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(_masterKey);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }

        public static string GenerateBase64Key32Bytes()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        }
    }
}
