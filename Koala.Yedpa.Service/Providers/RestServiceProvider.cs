using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Providers;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Koala.Yedpa.Service.Providers;

public class RestServiceProvider(IHttpClientFactory httpClientFactory) : IRestServiceProvider
{
    //private readonly HttpClient _client;

    //_client = client;

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPost(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
        try
        {
            var _client= httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri(baseUri);

            if (string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }
            var bContent = new StringContent(param, Encoding.UTF8, Application.Json);

            var res = await _client.PostAsync(url, bContent);
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                return ResponseDto<string>.SuccessData(200, "Get İşlemi Başarıyla Gerçekleştirildi", content);
            }
            else
            {
                var content = await res.Content.ReadAsStringAsync();
                var code = (int)res.StatusCode;
               
                return ResponseDto<string>.FailData(code, "Get İşlemi Sonrası Bir Hata Oluştu", content, true);
            }

        }
        catch (WebException webEx)
        {
            var response = (HttpWebResponse)webEx.Response!;
            //if (response == null)
            //{
            //    return ResponseDto<string>.FailData(503, "Lisans Sunucusuna Ulaşılamadı", webEx.Message, true);
            //}
            var content = new StreamReader(response.GetResponseStream());
            return ResponseDto<string>.FailData(400, content.ReadToEnd(), webEx.Message, true);
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
            var _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri(baseUri);
            var req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            req!.Method = "PUT";
            req.ContentType = "application/json";
            req.Accept = "application/json";


            if (string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }

            var formData = Encoding.UTF8.GetBytes(param);
            req.ContentLength = formData.Length;

            await using (var post = req.GetRequestStream())
            {
                post.Write(formData, 0, formData.Length);
            }

            using (var resp = req.GetResponse() as HttpWebResponse)
            {
                var reader = new StreamReader(resp!.GetResponseStream());
                return ResponseDto<string>.SuccessData(200, "Put işlemi başarıyla gerçekleştirildi", reader.ReadToEnd());
            }
        }
        catch (WebException webEx)
        {
            var response = (HttpWebResponse)webEx.Response!;
            var content = new StreamReader(response.GetResponseStream());
            return ResponseDto<string>.FailData(400, content.ReadToEnd(), webEx.Message, false);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.FailData(400, "Put İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, false);
        }
    }

    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpPatch(string baseUri, string url, string param, string token, string tokenHeaderName = "Token")
    {
        try
        {
            var req = WebRequest.Create(new Uri(url)) as HttpWebRequest;
            req!.Method = "PATCH";
            req.ContentType = "application/json";
            req.Accept = "application/json";
            var _client = httpClientFactory.CreateClient();
            if (string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }

            var formData = Encoding.UTF8.GetBytes(param);
            req.ContentLength = formData.Length;

            await using (var post = req.GetRequestStream())
            {
                await post.WriteAsync(formData, 0, formData.Length);
            }

            using (var resp = req.GetResponse() as HttpWebResponse)
            {
                var reader = new StreamReader(resp!.GetResponseStream());
                return ResponseDto<string>.SuccessData(200, "Patch İşlermi Başarıyla Gerçekleşitirildi", await reader.ReadToEndAsync());
            }
        }
        catch (WebException webEx)
        {
            var response = (HttpWebResponse)webEx.Response!;
            var content = new StreamReader(response.GetResponseStream());
            return ResponseDto<string>.FailData(400, await content.ReadToEndAsync(), webEx.Message, false);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.FailData(400, "Patch İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message, false);
        }
    }
    [Obsolete("Obsolete")]
    public async Task<ResponseDto<string>> HttpGet(string baseUri, string url, string token, string tokenHeaderName = "Token")
    {
        try
        {
            var _client = httpClientFactory.CreateClient();
            if (string.IsNullOrEmpty(token))
            {
                _client.DefaultRequestHeaders.Add(tokenHeaderName, token);
            }
            var res = await _client.GetAsync(url);
            if (res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                return ResponseDto<string>.SuccessData(200, "Get İşlemi Başarıyla Gerçekleştirildi", content);
            }
            else
            {
                var content = await res.Content.ReadAsStringAsync();
                return ResponseDto<string>.SuccessData((int)res.StatusCode, "Get İşlemi Sonrası Bir Hata Oluştu", content);
            }

        }
        catch (WebException webEx)
        {
            var response = (HttpWebResponse)webEx.Response!;
            var content = new StreamReader(response.GetResponseStream());
            return ResponseDto<string>.SuccessData(400, content.ReadToEnd(), webEx.Message);
        }
        catch (Exception ex)
        {
            return ResponseDto<string>.SuccessData(400, "Get İşlemi Gerçekleştirilirken Bir Sorunla Karşılaşıldı", ex.Message);
        }
    }
}