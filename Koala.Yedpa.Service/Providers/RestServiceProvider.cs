using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Providers;
using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace Koala.Yedpa.Service.Providers;

public class RestServiceProvider(IHttpClientFactory httpClientFactory) : IRestServiceProvider
{
    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPost(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
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

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPut(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
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

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPatch(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
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

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpGet(string baseUri, string url, string token, string tokenHeaderName = "Token")
    {
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
}
