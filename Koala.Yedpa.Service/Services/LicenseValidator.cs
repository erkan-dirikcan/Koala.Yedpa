using Koala.Yedpa.Core.Services;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Koala.Yedpa.Core.Helpers
{
    public class LicenseValidator : ILicenseValidator
    {
        private (string CustomerCode, string ApplicationId, DateTime? ExpirationDate)? _payload;

        private (string CustomerCode, string ApplicationId, DateTime? ExpirationDate)? GetLicenseInfo()
        {
            if (_payload.HasValue) return _payload;

            try
            {
                var basePath = Directory.GetCurrentDirectory();
                var licensePath = Path.Combine(basePath, "wwwroot", "Licenses", "license.lic");
                var privatePemPath = Path.Combine(basePath, "wwwroot", "Licenses", "Koala.Yedpa.Yonetim_private.pem");

                if (!File.Exists(licensePath) || !File.Exists(privatePemPath))
                    return null;

                var licenseBytes = File.ReadAllBytes(licensePath);
                var privatePemContent = File.ReadAllText(privatePemPath);

                using var rsa = RSA.Create();
                rsa.ImportFromPem(privatePemContent);

                var signatureSize = rsa.KeySize / 8;
                var encryptedLength = licenseBytes.Length - signatureSize;

                if (encryptedLength <= 0)
                    return null;

                var encrypted = licenseBytes.AsSpan(0, encryptedLength).ToArray();
                var signature = licenseBytes.AsSpan(encryptedLength).ToArray();

                // İmza doğrula (private key ile de doğrulanır)
                if (!rsa.VerifyData(encrypted, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    return null;

                // Private key ile çöz
                var decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
                var json = Encoding.UTF8.GetString(decrypted);

                var payload = JsonConvert.DeserializeAnonymousType(json, new
                {
                    CustomerCode = "",
                    ExpirationDate = "",
                    ApplicationId = ""
                });

                if (payload == null)
                    return null;

                DateTime? exp = DateTime.TryParse(payload.ExpirationDate, out var dt) ? dt : null;

                _payload = (payload.CustomerCode, payload.ApplicationId, exp);
                return _payload;
            }
            catch
            {
                return null;
            }
        }

        public bool IsLicenseValid()
        {
            var info = GetLicenseInfo(); // artık aynı instance metod, hata yok
            if (info == null) return false;

            if (info.Value.ExpirationDate.HasValue && DateTime.UtcNow > info.Value.ExpirationDate.Value)
                return false;

            return true;
        }

        public string? GetXKey() => GetLicenseInfo()?.CustomerCode;

        public string? GetApplicationId() => GetLicenseInfo()?.ApplicationId;
    }
}