using Microsoft.Extensions.Logging;
using Moq;
using Koala.Yedpa.Core.Repositories;

namespace Koala.Yedpa.Service.Tests;

/// <summary>
/// Base test fixture for service tests with common mock setup
/// </summary>
public abstract class ServiceTestBase
{
    protected Mock<ILogger<T>> CreateLoggerMock<T>() where T : class
    {
        return new Mock<ILogger<T>>();
    }
}

/// <summary>
/// Helper methods for creating test DTOs
/// </summary>
public static class TestDtoHelper
{
    public static Core.Dtos.Yonetim.ArsivDto CreateArsivDto(string rafKod = "RAF01", string bolmeNo = "B1", string koliNo = "K1")
    {
        return new Core.Dtos.Yonetim.ArsivDto
        {
            RafID = 1,
            RafKod = rafKod,
            BolmeID = 1,
            BolmeNo = bolmeNo,
            KoliID = 1,
            KoliNo = koliNo,
            Detay = "Test Detay"
        };
    }

    public static Core.Dtos.Yonetim.SozlesmeListDto CreateSozlesmeListDto(string firma = "Test Firma")
    {
        return new Core.Dtos.Yonetim.SozlesmeListDto
        {
            SozlesmeID = 1,
            Firma = firma,
            Konu = "Test Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Durum = "Aktif",
            KalanGun = 365
        };
    }

    public static Core.Dtos.Yonetim.ArizaListDto CreateArizaListDto(string konu = "Test Arıza")
    {
        return new Core.Dtos.Yonetim.ArizaListDto
        {
            ArizaID = 1,
            Konu = konu,
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            SonKisi = null,
            Gizli = false,
            HareketSayisi = 0
        };
    }

    public static Core.Dtos.Yonetim.OtoparkListDto CreateOtoparkListDto(string plaka = "34ABC123")
    {
        return new Core.Dtos.Yonetim.OtoparkListDto
        {
            KayitID = 1,
            Plaka = plaka,
            GirisTarih = DateTime.Now,
            CikisTarih = null,
            AboneAd = "Test Abone",
            Telefon = "5551234567"
        };
    }
}
