using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace Koala.Yedpa.Service.Providers;

public class LogoRestServiceProvider(IHttpClientFactory httpClientFactory, ILicenseReader licenseReader) : ILogoRestServiceProvider
{
    private string? _cachedAccessToken;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly ILicenseReader _licenseReader = licenseReader;

    public async Task<ResponseDto<string>> HttpGet(string baseUri, string url, string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Get İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Get İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException)
        {
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.FailData(400, "Get İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<string>> HttpPost(string baseUri, string url, string param, string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Post İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Post İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException)
        {
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.FailData(400, "Post İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<string>> HttpPut(string baseUri, string url, string param, string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Put İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Put İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException)
        {
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.FailData(400, "Put İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<string>> HttpPatch(string baseUri, string url, string param, string token)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return ResponseDto<string>.SuccessData(200, "Patch İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Patch İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException)
        {
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.FailData(400, "Patch İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    public async Task<ResponseDto<string>> GetAccessToken(string url, string userName, string password, string firmNr, string clientId, string clientSecret)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();

            // Basic Authentication header oluştur
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Form data oluştur (application/x-www-form-urlencoded)
            var formData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "password"),
                new("username", userName),
                new("firmno", firmNr),
                new("password", password)
            };

            var formContent = new FormUrlEncodedContent(formData);
            formContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");

            var response = await client.PostAsync(url, formContent);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                // JSON'dan access_token'ı al
                var tokenResponse = JsonConvert.DeserializeObject<dynamic>(responseContent);
                var accessToken = tokenResponse?.access_token?.ToString();

                if (string.IsNullOrEmpty(accessToken))
                {
                    return ResponseDto<string>.FailData(400, "Access Token alınamadı", "Sunucudan dönen yanıtta access_token bulunamadı", true);
                }

                _cachedAccessToken = accessToken;
                return ResponseDto<string>.SuccessData(200, "Access Token başarıyla alındı", accessToken);
            }
            else
            {
                // Hata durumunda sunucudan dönen detaylı hatayı döndür (Logo'nun döndüğü JSON hata mesajı)
                var statusCode = (int)response.StatusCode;
                return ResponseDto<string>.FailData(statusCode, "Access Token alınamadı", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            // .NET Core'da WebException yok, HttpRequestException kullanılıyor
            // Sunucudan dönen detaylı hata mesajını döndür
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException)
        {
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.FailData(400, "Access Token alınırken bir hata oluştu", ex.Message, true);
        }
    }
}
