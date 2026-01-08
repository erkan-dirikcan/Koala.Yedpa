using Koala.Yedpa.Core.Services;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace Koala.Yedpa.Core.Helpers
{
    public class LicenseValidator : ILicenseValidator
    {
        private (string CustomerCode, string ApplicationId, DateTime? ExpirationDate, string LogoClientId, string LogoClientSecret)? _payload;

        private (string CustomerCode, string ApplicationId, DateTime? ExpirationDate, string LogoClientId, string LogoClientSecret)? GetLicenseInfo()
        {
            if (_payload.HasValue) return _payload;

            try
            {
                Console.WriteLine("🔍 Lisans doğrulanıyor...");
                var basePath = Directory.GetCurrentDirectory();
                var licensePath = Path.Combine(basePath, "wwwroot", "Licenses", "license.lic");
                var privatePemPath = Path.Combine(basePath, "wwwroot", "Licenses", "Koala.Yedpa_private.pem");

                Console.WriteLine($"   BasePath: {basePath}");
                Console.WriteLine($"   LicensePath: {licensePath}");
                Console.WriteLine($"   PrivatePemPath: {privatePemPath}");

                if (!File.Exists(licensePath))
                {
                    Console.WriteLine("❌ Lisans dosyası BULUNAMADI!");
                    return null;
                }

                if (!File.Exists(privatePemPath))
                {
                    Console.WriteLine("❌ Private key dosyası BULUNAMADI!");
                    return null;
                }

                var licenseBytes = File.ReadAllBytes(licensePath);
                var privatePemContent = File.ReadAllText(privatePemPath);
                Console.WriteLine($"   License bytes: {licenseBytes.Length}");

                using var rsa = RSA.Create();
                rsa.ImportFromPem(privatePemContent);
                Console.WriteLine($"   RSA KeySize: {rsa.KeySize}");

                var signatureSize = rsa.KeySize / 8;
                var encryptedLength = licenseBytes.Length - signatureSize;

                if (encryptedLength <= 0)
                {
                    Console.WriteLine("❌ Lisans dosyası boyutu HATALI!");
                    return null;
                }

                var encrypted = licenseBytes.AsSpan(0, encryptedLength).ToArray();
                var signature = licenseBytes.AsSpan(encryptedLength).ToArray();

                Console.WriteLine($"   Encrypted: {encrypted.Length}, Signature: {signature.Length}");

                // İmza doğrula (private key ile de doğrulanır)
                bool signatureValid = rsa.VerifyData(encrypted, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                Console.WriteLine($"   İmza doğrulaması: {signatureValid}");

                if (!signatureValid)
                {
                    Console.WriteLine("❌ İmza doğrulaması BAŞARISIZ!");
                    return null;
                }

                // Private key ile çöz
                var decrypted = rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
                var json = Encoding.UTF8.GetString(decrypted);
                Console.WriteLine($"   Decrypted JSON length: {json.Length}");

                var payload = JsonConvert.DeserializeAnonymousType(json, new
                {
                    CustomerCode = "",
                    ExpirationDate = "",
                    ApplicationId = "",
                    CustomContent = "",  // Changed to string - it's stored as JSON string in the file
                    Mode = ""
                });

                if (payload == null)
                {
                    Console.WriteLine("❌ JSON deserileştirilemedi!");
                    return null;
                }

                Console.WriteLine($"   CustomerCode: {payload.CustomerCode}");
                Console.WriteLine($"   ApplicationId: {payload.ApplicationId}");
                Console.WriteLine($"   ExpirationDate: {payload.ExpirationDate}");
                Console.WriteLine($"   Mode: {payload.Mode}");
                Console.WriteLine($"   CustomContent: {(string.IsNullOrEmpty(payload.CustomContent) ? "❌ BOŞ" : "✅")}");

                // Parse CustomContent from JSON string to object
                string? logoClientId = "";
                string? logoClientSecret = "";

                if (!string.IsNullOrEmpty(payload.CustomContent))
                {
                    try
                    {
                        Console.WriteLine($"   CustomContent raw: {payload.CustomContent.Substring(0, Math.Min(100, payload.CustomContent.Length))}...");
                        var customContentObj = JsonConvert.DeserializeAnonymousType(payload.CustomContent, new
                        {
                            LogoClientId = "",
                            LogoClientSecret = ""
                        });
                        logoClientId = customContentObj?.LogoClientId ?? "";
                        logoClientSecret = customContentObj?.LogoClientSecret ?? "";
                        Console.WriteLine($"   LogoClientId: {(string.IsNullOrEmpty(logoClientId) ? "❌ BOŞ" : "✅")}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"   ⚠️ CustomContent parse hatası: {ex.Message}");
                    }
                }

                DateTime? exp = DateTime.TryParse(payload.ExpirationDate, out var dt) ? dt : null;

                _payload = (
                    payload.CustomerCode,
                    payload.ApplicationId,
                    exp,
                    logoClientId,
                    logoClientSecret
                );
                return _payload;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lisans okuma HATASI: {ex.Message}");
                Console.WriteLine($"   StackTrace: {ex.StackTrace}");
                return null;
            }
        }

        public bool IsLicenseValid()
        {
            var info = GetLicenseInfo();
            if (info == null)
            {
                Console.WriteLine("❌ LISANS HATASI: GetLicenseInfo() null döndü");
                Console.WriteLine("   - Lisans dosyası: " + File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licenses", "license.lic")));
                Console.WriteLine("   - Private key dosyası: " + File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Licenses", "Koala.Yedpa_private.pem")));
                return false;
            }

            if (info.Value.ExpirationDate.HasValue && DateTime.UtcNow > info.Value.ExpirationDate.Value)
            {
                Console.WriteLine($"❌ LISANS HATASI: Lisans süresi dolmuş! Bitiş tarihi: {info.Value.ExpirationDate.Value}");
                return false;
            }

            Console.WriteLine($"✅ Lisans GEÇERLİ: {info.Value.CustomerCode}, {info.Value.ApplicationId}");
            return true;
        }

        public string? GetXKey() => GetLicenseInfo()?.CustomerCode;

        public string? GetApplicationId() => GetLicenseInfo()?.ApplicationId;

        public string? GetLogoClientId() => GetLicenseInfo()?.LogoClientId;

        public string? GetLogoClientSecret() => GetLicenseInfo()?.LogoClientSecret;
    }
}