using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Koala.Yedpa.Core.Helpers
{
    public static class LicenseFileHelper
    {
        private static readonly string LicensePath = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "Licenses", "license.lic");

        private static readonly string PublicPemPath = Path.Combine(
            Directory.GetCurrentDirectory(), "wwwroot", "Licenses", "Koala.Yedpa.Yonetim_public.pem");

        public static (string CustomerCode, string ApplicationId, DateTime? ExpirationDate, string? LogoClientId, string? LogoClientSecret)? ReadLicensePayload()
        {
            var licensePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licenses", "license.lic");
            var privatePemPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licenses", "Koala.Yedpa_private.pem");
            var publicPemPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licenses", "Koala.Yedpa_public.pem");

            if (!File.Exists(licensePath) || !File.Exists(privatePemPath) || !File.Exists(publicPemPath))
                return null;

            var licenseBytes = File.ReadAllBytes(licensePath);
            var privatePem = File.ReadAllText(privatePemPath);
            var publicPem = File.ReadAllText(publicPemPath);

            using var rsaPrivate = RSA.Create();
            rsaPrivate.ImportFromPem(privatePem);

            using var rsaPublic = RSA.Create();
            rsaPublic.ImportFromPem(publicPem);

            var signatureSize = rsaPrivate.KeySize / 8;
            var encryptedLength = licenseBytes.Length - signatureSize;

            var encrypted = licenseBytes.AsSpan(0, encryptedLength).ToArray();
            var signature = licenseBytes.AsSpan(encryptedLength).ToArray();

            // PUBLIC KEY İLE İMZA DOĞRULA
            if (!rsaPublic.VerifyData(encrypted, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                return null;

            // PRIVATE KEY İLE ŞİFREYİ ÇÖZ
            var decrypted = rsaPrivate.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
            var json = Encoding.UTF8.GetString(decrypted);

            var payload = JsonConvert.DeserializeAnonymousType(json, new
            {
                CustomerCode = "",
                ExpirationDate = "",
                ApplicationId = "",
                CustomContent = new
                {
                    LogoClientId = "",
                    LogoClientSecret = ""
                },
                Mode = ""
            });

            if (payload == null) return null;

            DateTime? exp = DateTime.TryParse(payload.ExpirationDate, out var dt) ? dt : null;

            var logoClientId = payload.CustomContent?.LogoClientId;
            var logoClientSecret = payload.CustomContent?.LogoClientSecret;

            return (payload.CustomerCode, payload.ApplicationId, exp, logoClientId, logoClientSecret);
        }
    }
}