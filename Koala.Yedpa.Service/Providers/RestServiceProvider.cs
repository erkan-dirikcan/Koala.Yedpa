using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Providers;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Koala.Yedpa.Service.Providers;

public class RestServiceProvider(IHttpClientFactory httpClientFactory, ILogger<RestServiceProvider> logger) : IRestServiceProvider
{
    private readonly ILogger<RestServiceProvider> _logger = logger;

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPost(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
        _logger.LogDebug("HttpPost called: BaseUri={BaseUri}, Url={Url}, HasToken={HasToken}", baseUri, url, !string.IsNullOrEmpty(token));
        try
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);

            // Token varsa header'a ekle
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Remove(tokenHeaderName);
                client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("HttpPost: Success - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, (int)response.StatusCode);
                return ResponseDto<string>.SuccessData(200, "Post İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning("HttpPost: Failed - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, statusCode);
                return ResponseDto<string>.FailData(statusCode, "Post İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HttpPost: HttpRequestException - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException tcEx)
        {
            _logger.LogWarning(tcEx, "HttpPost: Request timeout - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpPost: Unexpected error - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(400, "Post İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPut(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
        _logger.LogDebug("HttpPut called: BaseUri={BaseUri}, Url={Url}, HasToken={HasToken}", baseUri, url, !string.IsNullOrEmpty(token));
        try
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);

            // Token varsa header'a ekle
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Remove(tokenHeaderName);
                client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PutAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("HttpPut: Success - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, (int)response.StatusCode);
                return ResponseDto<string>.SuccessData(200, "Put İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning("HttpPut: Failed - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, statusCode);
                return ResponseDto<string>.FailData(statusCode, "Put İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HttpPut: HttpRequestException - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException tcEx)
        {
            _logger.LogWarning(tcEx, "HttpPut: Request timeout - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpPut: Unexpected error - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(400, "Put İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPatch(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
        _logger.LogDebug("HttpPatch called: BaseUri={BaseUri}, Url={Url}, HasToken={HasToken}", baseUri, url, !string.IsNullOrEmpty(token));
        try
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);

            // Token varsa header'a ekle
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Remove(tokenHeaderName);
                client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }

            var content = new StringContent(param, Encoding.UTF8, "application/json");
            var response = await client.PatchAsync(url, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("HttpPatch: Success - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, (int)response.StatusCode);
                return ResponseDto<string>.SuccessData(200, "Patch İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning("HttpPatch: Failed - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, statusCode);
                return ResponseDto<string>.FailData(statusCode, "Patch İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HttpPatch: HttpRequestException - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException tcEx)
        {
            _logger.LogWarning(tcEx, "HttpPatch: Request timeout - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpPatch: Unexpected error - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(400, "Patch İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpGet(string baseUri, string url, string token, string tokenHeaderName = "Token")
    {
        _logger.LogDebug("HttpGet called: BaseUri={BaseUri}, Url={Url}, HasToken={HasToken}", baseUri, url, !string.IsNullOrEmpty(token));
        try
        {
            var client = httpClientFactory.CreateClient();
            client.BaseAddress = new Uri(baseUri);

            // Token varsa header'a ekle
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Remove(tokenHeaderName);
                client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }

            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("HttpGet: Success - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, (int)response.StatusCode);
                return ResponseDto<string>.SuccessData(200, "Get İşlemi Başarıyla Gerçekleştirildi", responseContent);
            }
            else
            {
                var statusCode = (int)response.StatusCode;
                _logger.LogWarning("HttpGet: Failed - BaseUri={BaseUri}, Url={Url}, StatusCode={StatusCode}", baseUri, url, statusCode);
                return ResponseDto<string>.FailData(statusCode, "Get İşlemi Sonrası Bir Hata Oluştu", responseContent, true);
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HttpGet: HttpRequestException - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(503, "Sunucuya Ulaşılamadı", httpEx.Message, true);
        }
        catch (TaskCanceledException tcEx)
        {
            _logger.LogWarning(tcEx, "HttpGet: Request timeout - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(408, "İstek Zaman Aşımına Uğradı", "Request timeout", true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "HttpGet: Unexpected error - BaseUri={BaseUri}, Url={Url}", baseUri, url);
            return ResponseDto<string>.FailData(400, "Get İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, true);
        }
    }
}
