using Koala.Yedpa.Core.Exceptions;
using Koala.Yedpa.Core.Services;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace Koala.Yedpa.Service.Services // <-- SENİN NAMESPACE'İN
{
    public class CryptoService : ICryptoService
    {
        private readonly HttpClient _httpClient;
        private readonly ILicenseReader _licenseReader;
        private string? _cachedAppId;

        public CryptoService(HttpClient httpClient, ILicenseReader licenseReader)
        {
            _httpClient = httpClient;
            _licenseReader = licenseReader;
        }

        public async Task<string> EncryptAsync(string plainText)
            => await CallCryptoApi("encrypt", plainText);

        public async Task<string> DecryptAsync(string cipherText)
            => await CallCryptoApi("decrypt", cipherText);


        private async Task<string> CallCryptoApi(string action, string data)
        {
            try
            {
                _cachedAppId ??= await _licenseReader.GetApplicationIdAsync();
                if (string.IsNullOrEmpty(_cachedAppId))
                    return string.Empty;

                // DİKKAT: encrypt → plainText
                //         decrypt → cryptedText
                object request = action == "encrypt"
                    ? new { applicationId = _cachedAppId, plainText = data }
                    : new { applicationId = _cachedAppId, cryptedText = data };

                var response = await _httpClient.PostAsJsonAsync($"api/Crypto/{action}", request);

                // 403 hatası (Lisans hatası) kontrolü
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    string errorMessage = "Lisans hatası oluştu.";
                    
                    try
                    {
                        // Sunucudan dönen JSON formatındaki hata mesajını parse et
                        var errorObj = JsonConvert.DeserializeObject<dynamic>(errorResponse);
                        if (errorObj?.message != null)
                        {
                            errorMessage = errorObj.message.ToString();
                        }
                    }
                    catch
                    {
                        // JSON parse edilemezse, ham hata mesajını kullan
                        if (!string.IsNullOrWhiteSpace(errorResponse))
                        {
                            errorMessage = errorResponse;
                        }
                    }
                    
                    throw new CryptoLicenseException(errorMessage);
                }

                if (!response.IsSuccessStatusCode)
                    return string.Empty;

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // API encrypt ise encryptedText, decrypt ise plainText dönüyor
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (action == "encrypt")
                    return result?.encryptedText?.ToString() ?? string.Empty;
                else
                    return result?.plainText?.ToString() ?? string.Empty;
            }
            catch (CryptoLicenseException)
            {
                // Lisans hatasını yukarı fırlat (yakalanmasın)
                throw;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}