using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.KrediKartTahsilat;

namespace Koala.Yedpa.Core.Services;

/// <summary>
/// Kredi kartı tahsilatları servisi arayüzü.
/// Logo Tiger REST API ArpSlips endpoint'ine kredi kartı tahsilat fişi (TYPE=70) oluşturur.
/// </summary>
public interface IKrediKartTahsilatService
{
    /// <summary>
    /// Dışarıdan alınan input modelini (Model1) Logo ArpSlips payload'ına (Model2) mapleyip
    /// Logo Tiger REST API üzerinden kredi kartı tahsilat fişi oluşturur.
    /// </summary>
    /// <param name="request">Kredi kartı tahsilat input modeli</param>
    /// <returns>Logo'dan dönen ham response</returns>
    Task<ResponseDto<string>> CreateKrediKartTahsilatAsync(CreateKrediKartTahsilatRequestDto request);
}
