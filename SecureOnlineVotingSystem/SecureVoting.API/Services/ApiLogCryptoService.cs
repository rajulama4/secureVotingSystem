using System.Security.Cryptography;
using System.Text;

namespace SecureVoting.API.Services
{
    // Stores: [12-byte nonce | 16-byte tag | ciphertext]
    public class ApiLogCryptoService
    {
        private readonly byte[] _key;

        public ApiLogCryptoService(IConfiguration config)
        {
            var b64 = config["ApiLoggingCrypto:KeyBase64"];
            if (string.IsNullOrWhiteSpace(b64))
                throw new InvalidOperationException("ApiLoggingCrypto:KeyBase64 missing.");

            _key = Convert.FromBase64String(b64);

            if (_key.Length != 32)
                throw new InvalidOperationException("ApiLoggingCrypto key must be 32 bytes (Base64 of 32 bytes).");
        }

        public byte[] EncryptString(string? plaintext)
        {
            plaintext ??= "";
            var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);

            var nonce = RandomNumberGenerator.GetBytes(12);   // 12 bytes nonce
            var tag = new byte[16];                           // 16 bytes tag
            var ciphertext = new byte[plaintextBytes.Length];

            using var aes = new AesGcm(_key);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            // pack => nonce + tag + ciphertext
            var packed = new byte[12 + 16 + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, packed, 0, 12);
            Buffer.BlockCopy(tag, 0, packed, 12, 16);
            Buffer.BlockCopy(ciphertext, 0, packed, 28, ciphertext.Length);

            return packed;
        }

        public string DecryptToString(byte[]? packed)
        {
            if (packed == null || packed.Length < 28) return "";

            var nonce = new byte[12];
            var tag = new byte[16];
            var ciphertext = new byte[packed.Length - 28];

            Buffer.BlockCopy(packed, 0, nonce, 0, 12);
            Buffer.BlockCopy(packed, 12, tag, 0, 16);
            Buffer.BlockCopy(packed, 28, ciphertext, 0, ciphertext.Length);

            var plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(_key);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}