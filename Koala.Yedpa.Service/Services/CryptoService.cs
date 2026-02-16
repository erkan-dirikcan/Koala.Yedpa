using Koala.Yedpa.Core.Exceptions;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Json;

namespace Koala.Yedpa.Service.Services // <-- SENİN NAMESPACE'İN
{
    public class CryptoService : ICryptoService
    {
        private readonly HttpClient _httpClient;
        private readonly ILicenseReader _licenseReader;
        private readonly ILogger<CryptoService> _logger;
        private string? _cachedAppId;

        public CryptoService(HttpClient httpClient, ILicenseReader licenseReader, ILogger<CryptoService> logger)
        {
            _httpClient = httpClient;
            _licenseReader = licenseReader;
            _logger = logger;
        }

        public async Task<string> EncryptAsync(string plainText)
            => await CallCryptoApi("encrypt", plainText);

        public async Task<string> DecryptAsync(string cipherText)
            => await CallCryptoApi("decrypt", cipherText);


        private async Task<string> CallCryptoApi(string action, string data)
        {
            _logger.LogDebug("CallCryptoApi: Action={Action}, DataLength={DataLength}", action, data?.Length ?? 0);
            try
            {
                _cachedAppId ??= await _licenseReader.GetApplicationIdAsync();
                if (string.IsNullOrEmpty(_cachedAppId))
                {
                    _logger.LogWarning("CallCryptoApi: ApplicationId is null or empty");
                    return string.Empty;
                }

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

                    _logger.LogError("CallCryptoApi: License error - {ErrorMessage}", errorMessage);
                    throw new CryptoLicenseException(errorMessage);
                }

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("CallCryptoApi: API call failed with status {StatusCode}", response.StatusCode);
                    return string.Empty;
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // API encrypt ise encryptedText, decrypt ise plainText dönüyor
                var result = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                if (action == "encrypt")
                {
                    var encryptedText = result?.encryptedText?.ToString() ?? string.Empty;
                    var length = (int)encryptedText.Length;
                    _logger.LogDebug("CallCryptoApi: Encrypt successful, result length={Length}", length);
                    return encryptedText;
                }
                else
                {
                    var plainText = result?.plainText?.ToString() ?? string.Empty;
                    var length = (int)plainText.Length;
                    _logger.LogDebug("CallCryptoApi: Decrypt successful, result length={Length}", length);
                    return plainText;
                }
            }
            catch (CryptoLicenseException)
            {
                // Lisans hatasını yukarı fırlat (yakalanmasın)
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CallCryptoApi: Unexpected error for action={Action}", action);
                return string.Empty;
            }
        }
    }
}