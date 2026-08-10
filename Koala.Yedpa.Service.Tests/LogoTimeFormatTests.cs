using FluentAssertions;
using Koala.Yedpa.Core.Helpers;
using Xunit;

namespace Koala.Yedpa.Service.Tests;

/// <summary>
/// Logo'nun paketlenmiş TIME formatı: saat(2^24) · dakika(2^16) · saniye(2^8) · salise(1).
/// Beklenen değerler YEDPA canlı Logo verisiyle doğrulanmıştır (LG_211_16_CLFICHE.TIME,
/// 14.613 kayıt, aralık 00:01:00–23:54:48). Referans: `logo-time-format` skill'i.
///
/// Bu testler bir regresyon kalkanıdır: TIME yanlış yazılırsa Logo hata VERMEZ,
/// fiş oluşur ve saat sessizce saçma kaydedilir. Yıllarca fark edilmeyebilir.
/// </summary>
public class LogoTimeFormatTests
{
    [Theory]
    [InlineData(0, 0, 0, 0)]
    [InlineData(0, 1, 0, 65536)]
    [InlineData(9, 42, 10, 153750016)]
    [InlineData(12, 40, 14, 203951616)]
    [InlineData(16, 12, 49, 269234432)]
    [InlineData(23, 59, 59, 389757696)]
    public void ConvertToLogoTime_PaketlenmisDegerUretir(int saat, int dakika, int saniye, int beklenen)
    {
        new DateTime(2026, 8, 1, saat, dakika, saniye).ConvertToLogoTime().Should().Be(beklenen);
    }

    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(9, 42, 10)]
    [InlineData(12, 40, 14)]
    [InlineData(23, 59, 59)]
    public void ConvertToLogoTime_GidisDonusAyniDegeriVerir(int saat, int dakika, int saniye)
    {
        var packed = Tools.ConvertToLogoTime(saat, dakika, saniye);
        Tools.ParseLogoTime(packed).Should().Be((saat, dakika, saniye));
    }

    /// <summary>
    /// Canlı Logo kaydı: ArpSlips/17082 → TIME=269228595, HOUR=16, MINUTE=12.
    /// Saat ve dakika fişin HOUR/MINUTE alanlarıyla birebir örtüşüyor.
    ///
    /// DİKKAT: TIME'ın saniye byte'ı (26) fişin SEC_CREATED alanından (49) FARKLI.
    /// TIME fişin kendi saati, SEC_CREATED ise kaydın oluşturulma anı — ikisi ayrı bilgi.
    /// Bu test o ayrımı sabitliyor.
    /// </summary>
    [Fact]
    public void ParseLogoTime_CanliKaydiDogruCozer()
    {
        Tools.ParseLogoTime(269228595).Should().Be((16, 12, 26));
    }

    /// <summary>
    /// Eski hatalı formül (HHmmss) bir daha kullanılmasın: 12:40:14 için 124014 üretiyordu,
    /// Logo bunu 00:01:228 diye okuyordu — geçersiz bir saat.
    /// </summary>
    [Fact]
    public void EskiHHmmssFormulu_GecersizSaatUretiyordu()
    {
        const int eskiHataliDeger = 12 * 10000 + 40 * 100 + 14;   // 124014
        var (saat, dakika, saniye) = Tools.ParseLogoTime(eskiHataliDeger);

        saat.Should().Be(0);
        saniye.Should().BeGreaterThan(59, "saniye 59'u aşıyorsa değer paketlenmiş format değildir");
        new DateTime(2026, 8, 1, 12, 40, 14).ConvertToLogoTime().Should().NotBe(eskiHataliDeger);
    }
}
