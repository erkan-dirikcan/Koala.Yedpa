using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.LogoJsonModels;

namespace Koala.Yedpa.Core.Providers;

public interface ILogoRestServiceProvider
{
    Task<ResponseDto<string>> HttpGet(string url);
    Task<ResponseDto<string>> HttpPost(string url, string param);
    Task<ResponseDto<string>> HttpPut(string url, string param);
    Task<ResponseDto<string>> HttpPatch(string url, string param);

    /// <summary>
    /// Logo'ya SalesOrder gönder
    /// </summary>
    Task<ResponseDto<string>> PostSalesOrderAsync(SalesOrderJsonViewModel salesOrder);

    /// <summary>
    /// Logo REST API servisini kontrol et (token gerektirmez)
    /// </summary>
    Task<ResponseDto<string>> PingAsync();
}
