using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.LogoJsonModels;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Net;

namespace Koala.Yedpa.Service.Providers;

public class LogoRestServiceProvider : ILogoRestServiceProvider
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISettingsService _settingsService;
    private readonly ILicenseReader _licenseReader;
    private readonly ILogger<LogoRestServiceProvider> _logger;

    public LogoRestServiceProvider(
        IHttpClientFactory httpClientFactory,
        ISettingsService settingsService,
        ILicenseReader licenseReader,
        ILogger<LogoRestServiceProvider> logger)
    {
        _httpClientFactory = httpClientFactory;
        _settingsService = settingsService;
        _licenseReader = licenseReader;
        _logger = logger;
    }

    public async Task<ResponseDto<string>> HttpGet(string url)
    {
        return await ExecuteWithAuthAsync(async (token) =>
        {
            var settings = await _settingsService.GetLogoRestServiceSettingsAsync();
            if (!settings.IsSuccess || settings.Data == null)
            {
                return ResponseDto<string>.FailData(500, "Ayarlar alınamadı", "Logo REST ayarları bulunamadı", true);
            }

            var baseUri = $"http://{settings.Data.Server}:{settings.Data.Port}/api/v1/";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Get işlemi başarılı", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Get işlemi başarısız", responseContent, true);
            }
        });
    }

    public async Task<ResponseDto<string>> HttpPost(string url, string param)
    {
        return await ExecuteWithAuthAsync(async (token) =>
        {
            var settings = await _settingsService.GetLogoRestServiceSettingsAsync();
            if (!settings.IsSuccess || settings.Data == null)
            {
                return ResponseDto<string>.FailData(500, "Ayarlar alınamadı", "Logo REST ayarları bulunamadı", true);
            }

            var baseUri = $"http://{settings.Data.Server}:{settings.Data.Port}/api/v1/";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Post işlemi başarılı", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Post işlemi başarısız", responseContent, true);
            }
        });
    }

    public async Task<ResponseDto<string>> HttpPut(string url, string param)
    {
        return await ExecuteWithAuthAsync(async (token) =>
        {
            var settings = await _settingsService.GetLogoRestServiceSettingsAsync();
            if (!settings.IsSuccess || settings.Data == null)
            {
                return ResponseDto<string>.FailData(500, "Ayarlar alınamadı", "Logo REST ayarları bulunamadı", true);
            }

            var baseUri = $"http://{settings.Data.Server}:{settings.Data.Port}/api/v1/";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Put işlemi başarılı", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Put işlemi başarısız", responseContent, true);
            }
        });
    }

    public async Task<ResponseDto<string>> HttpPatch(string url, string param)
    {
        return await ExecuteWithAuthAsync(async (token) =>
        {
            var settings = await _settingsService.GetLogoRestServiceSettingsAsync();
            if (!settings.IsSuccess || settings.Data == null)
            {
                return ResponseDto<string>.FailData(500, "Ayarlar alınamadı", "Logo REST ayarları bulunamadı", true);
            }

            var baseUri = $"http://{settings.Data.Server}:{settings.Data.Port}/api/v1/";
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(url, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Patch işlemi başarılı", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Patch işlemi başarısız", responseContent, true);
            }
        });
    }

    private async Task<ResponseDto<string>> GetAccessTokenAsync()
    {
        try
        {
            // 1. Logo REST ayarlarını al
            var settingsResponse = await _settingsService.GetLogoRestServiceSettingsAsync();
            if (!settingsResponse.IsSuccess || settingsResponse.Data == null)
            {
                return ResponseDto<string>.FailData(500, "Ayarlar alınamadı", "Logo REST ayarları bulunamadı", true);
            }

            var settings = settingsResponse.Data;

            // 2. License dosyasından ClientId ve ClientSecret al
            var clientId = await _licenseReader.GetLogoClientIdAsync();
            var clientSecret = await _licenseReader.GetLogoClientSecretAsync();

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                return ResponseDto<string>.FailData(500, "License bilgileri alınamadı", "ClientId veya ClientSecret bulunamadı", true);
            }

            // 3. Token URL oluştur
            var baseUri = $"http://{settings.Server}:{settings.Port}/api/v1/";
            var tokenUrl = $"{baseUri}/token";

            // 4. HTTP client oluştur
            var client = _httpClientFactory.CreateClient();

            // 5. Basic Authentication header oluştur
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // 6. Form data oluştur (application/x-www-form-urlencoded)
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("username", settings.UserName),
                new("firmno", settings.Firm),
                new("password", settings.Password)
            };

            var formContent = new FormUrlEncodedContent(formData);

            // 7. Token al
            var response = await client.PostAsync(tokenUrl, formContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // 8. JSON'dan access_token'ı al
                var tokenResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var accessToken = tokenResponse?.access_token?.ToString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return ResponseDto<string>.FailData(400, "Token alınamadı", "Sunucudan dönen yanıtta access_token bulunamadı", true);
                }

                _logger.LogInformation("Access Token başarıyla alındı");
                return ResponseDto<string>.SuccessData(200, "Token alındı", accessToken);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Token alınamadı", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "GetAccessTokenAsync HTTP hatası: {Message}", httpEx.Message);
            return ResponseDto<string>.FailData(500, "Token bağlantı hatası", httpEx.Message, true);
        }
        catch (WebException webEx)
        {
            var errorResponse = ((HttpWebResponse)webEx.Response);
            string result = string.Empty;

            if (errorResponse != null)
            {
                using (var stream = errorResponse.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            _logger.LogError(webEx, "GetAccessTokenAsync WebException: {Message}, Response: {Response}", webEx.Message, result);
            return ResponseDto<string>.FailData(500, "Token web hatası", result, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetAccessTokenAsync genel hata: {Message}", ex.Message);
            return ResponseDto<string>.FailData(500, "Token hatası", ex.Message, true);
        }
    }

    private async Task<ResponseDto<bool>> RevokeTokenAsync(string accessToken)
    {
        try
        {
            var settingsResponse = await _settingsService.GetLogoRestServiceSettingsAsync();
            if (!settingsResponse.IsSuccess || settingsResponse.Data == null)
            {
                return ResponseDto<bool>.FailData(500, "Ayarlar alınamadı", "", true);
            }

            var settings = settingsResponse.Data;
            var baseUri = $"http://{settings.Server}:{settings.Port}";
            var revokeUrl = $"{baseUri}/Revoke";

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PostAsync(revokeUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Token başarıyla revoke edildi");
                return ResponseDto<bool>.SuccessData(200, "Token revoke edildi", true);
            }
            else
            {
                _logger.LogWarning("Token revoke edilemedi. StatusCode: {StatusCode}", response.StatusCode);
                // Revoke başarısız olsa bile işlem devam etsin
                return ResponseDto<bool>.SuccessData(200, "Token revoke edilemedi ama devam ediliyor", true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogWarning(httpEx, "Token revoke HTTP hatası: {Message}", httpEx.Message);
            // Revoke başarısız olsa bile işlem devam etsin
            return ResponseDto<bool>.SuccessData(200, "Token revoke hatası ama devam ediliyor", true);
        }
        catch (WebException webEx)
        {
            var errorResponse = ((HttpWebResponse)webEx.Response);
            string result = string.Empty;

            if (errorResponse != null)
            {
                using (var stream = errorResponse.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            _logger.LogWarning(webEx, "Token revoke WebException: {Message}, Response: {Response}", webEx.Message, result);
            // Revoke başarısız olsa bile işlem devam etsin
            return ResponseDto<bool>.SuccessData(200, "Token revoke hatası ama devam ediliyor", true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Token revoke genel hata: {Message}", ex.Message);
            // Revoke başarısız olsa bile işlem devam etsin
            return ResponseDto<bool>.SuccessData(200, "Token revoke hatası ama devam ediliyor", true);
        }
    }

    private async Task<ResponseDto<string>> ExecuteWithAuthAsync(Func<string, Task<ResponseDto<string>>> httpOperation)
    {
        string? token = null;

        try
        {
            // 1. Token al
            var authResponse = await GetAccessTokenAsync();
            if (!authResponse.IsSuccess)
            {
                return ResponseDto<string>.FailData(401, "Authentication failed", authResponse.Message, true);
            }

            token = authResponse.Data;

            // 2. HTTP işlemi yap
            var response = await httpOperation(token);

            // 3. Token revoke et (her durumda)
            try
            {
                await RevokeTokenAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Token revoke edilemedi");
            }

            return response;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "ExecuteWithAuthAsync HTTP hatası: {Message}", httpEx.Message);

            // Hata durumunda da token'i revoke etmeyi dene
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    await RevokeTokenAsync(token);
                }
                catch
                {
                    // Ignore
                }
            }

            return ResponseDto<string>.FailData(500, "İşlem başarısız", httpEx.Message, true);
        }
        catch (WebException webEx)
        {
            var errorResponse = ((HttpWebResponse)webEx.Response);
            string result = string.Empty;

            if (errorResponse != null)
            {
                using (var stream = errorResponse.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            _logger.LogError(webEx, "ExecuteWithAuthAsync WebException: {Message}, Response: {Response}", webEx.Message, result);

            // Hata durumunda da token'i revoke etmeyi dene
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    await RevokeTokenAsync(token);
                }
                catch
                {
                    // Ignore
                }
            }

            return ResponseDto<string>.FailData(500, "İşlem başarısız", result, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ExecuteWithAuthAsync genel hata: {Message}", ex.Message);

            // Hata durumunda da token'i revoke etmeyi dene
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    await RevokeTokenAsync(token);
                }
                catch
                {
                    // Ignore
                }
            }

            return ResponseDto<string>.FailData(500, "İşlem başarısız", ex.Message, true);
        }
    }

    public async Task<ResponseDto<string>> PostSalesOrderAsync(SalesOrderJsonViewModel salesOrder)
    {
        try
        {
            // SalesOrder'u JSON'a çevir
            var json = JsonConvert.SerializeObject(salesOrder, Formatting.Indented);
            _logger.LogInformation("SalesOrder gönderiliyor: {Json}", json);

            // salesOrders endpoint'ine gönder
            var response = await HttpPost("salesOrders", json);

            if (response.IsSuccess)
            {
                _logger.LogInformation("SalesOrder başarıyla gönderildi: {Data}", response.Data);
                return ResponseDto<string>.SuccessData(200, "SalesOrder başarıyla oluşturuldu", response.Data);
            }
            else
            {
                _logger.LogError("SalesOrder gönderilemedi: {Message}", response.Message);
                return ResponseDto<string>.FailData(response.StatusCode, "SalesOrder oluşturulamadı", response.Message, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "PostSalesOrderAsync HTTP hatası: {Message}", httpEx.Message);
            return ResponseDto<string>.FailData(500, "SalesOrder bağlantı hatası", httpEx.Message, true);
        }
        catch (WebException webEx)
        {
            var errorResponse = ((HttpWebResponse)webEx.Response);
            string result = string.Empty;

            if (errorResponse != null)
            {
                using (var stream = errorResponse.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
            }

            _logger.LogError(webEx, "PostSalesOrderAsync WebException: {Message}, Response: {Response}", webEx.Message, result);
            return ResponseDto<string>.FailData(500, "SalesOrder web hatası", result, true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PostSalesOrderAsync genel hata: {Message}", ex.Message);
            return ResponseDto<string>.FailData(500, "SalesOrder gönderim hatası", ex.Message, true);
        }
    }
}
