using Koala.Yedpa.Core.Dtos;
namespace Koala.Yedpa.Core.Providers;

public interface IRestServiceProvider
{
    Task<ResponseDto<string>> HttpPost(string baseUri, string url, string param, string Token, string tokenHeaderName = "Token");
    Task<ResponseDto<string>> HttpPut(string baseUri, string url, string param, string Token, string tokenHeaderName = "Token");
    Task<ResponseDto<string>> HttpPatch(string baseUri, string url, string param, string Token, string tokenHeaderName = "Token");
    Task<ResponseDto<string>> HttpGet(string baseUri, string url, string Token, string tokenHeaderName = "Token");


}