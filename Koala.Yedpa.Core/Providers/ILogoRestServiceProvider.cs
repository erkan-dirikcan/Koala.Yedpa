using Koala.Yedpa.Core.Dtos;

namespace Koala.Yedpa.Core.Providers;

public interface ILogoRestServiceProvider
{
    Task<ResponseDto<string>> HttpGet(string baseUri, string url, string token);
    Task<ResponseDto<string>> HttpPost(string baseUri, string url, string param, string token);
    Task<ResponseDto<string>> HttpPut(string baseUri, string url, string param, string token);
    Task<ResponseDto<string>> HttpPatch(string baseUri, string url, string param, string token);
}
