using OtpNet;
using QRCoder;

namespace SecureVoting.API.Services
{
    public class TotpService
    {
        public (string secretBase32, string otpauthUri, string qrBase64Png) GenerateSetup(string email, string issuer)
        {
            // 20 bytes = 160-bit secret (common recommendation)
            var secretBytes = KeyGeneration.GenerateRandomKey(20);
            var secretBase32 = Base32Encoding.ToString(secretBytes);

            var label = Uri.EscapeDataString($"{issuer}:{email}");
            var iss = Uri.EscapeDataString(issuer);

            var otpauthUri =
                $"otpauth://totp/{label}?secret={secretBase32}&issuer={iss}&digits=6&period=30";

            using var qrGen = new QRCodeGenerator();
            using var qrData = qrGen.CreateQrCode(otpauthUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrData);
            var pngBytes = qrCode.GetGraphic(10);

            return (secretBase32, otpauthUri, Convert.ToBase64String(pngBytes));
        }

        public bool Verify(string secretBase32, string code)
        {
            var secretBytes = Base32Encoding.ToBytes(secretBase32);
            var totp = new Totp(secretBytes, step: 30, totpSize: 6);

            // allow ±30s clock drift
            return totp.VerifyTotp(code, out _, new VerificationWindow(previous: 1, future: 1));
        }
    }
}
