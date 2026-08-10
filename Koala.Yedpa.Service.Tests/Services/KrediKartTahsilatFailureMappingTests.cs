using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.KrediKartTahsilat;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Koala.Yedpa.Service.Tests.Services;

/// <summary>
/// Logo hatalarının dışarıya nasıl yansıdığını ve fiş numarası sınırını doğrular.
///
/// Bağlam (01.08.2026): Logo'nun 401'i aynen dışarı verildiği için müşteri saatlerce
/// kendi token'ını araştırdı — oysa reddedilen bizim Logo token'ımızdı. Yukarı akış
/// hatası artık 502'ye çevriliyor; 401 yalnızca "senin token'ın geçersiz" demeli.
/// </summary>
public class KrediKartTahsilatFailureMappingTests : ServiceTestBase
{
    private readonly Mock<ILogoRestServiceProvider> _logo = new();
    private readonly IKrediKartTahsilatService _sut;

    public KrediKartTahsilatFailureMappingTests()
        => _sut = new KrediKartTahsilatService(_logo.Object, CreateLoggerMock<KrediKartTahsilatService>().Object);

    private static CreateKrediKartTahsilatRequestDto Istek(string? number = null) => new()
    {
        CollectionDate = new DateTime(2026, 8, 1, 12, 40, 14),
        BankAccCode = "02    0002",
        Number = number,
        Items = new List<KrediKartTahsilatItemDto>
        {
            new() { CustomerCode = "1.G000.254.AS.K4", Amount = 2535m }
        }
    };

    private void LogoDoner(int statusCode, string message)
        => _logo.Setup(x => x.HttpPost("ArpSlips", It.IsAny<string>()))
                .ReturnsAsync(ResponseDto<string>.FailData(statusCode, message, message, true));

    [Theory]
    [InlineData(401)]
    [InlineData(403)]
    public async Task LogoKimlikHatasi_Disariya502Doner(int logoStatus)
    {
        LogoDoner(logoStatus, "Unauthorized");

        var result = await _sut.CreateKrediKartTahsilatAsync(Istek());

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(502, "Logo'nun kimlik hatası çağıranın token'ıyla ilgili değildir");
        result.Message.Should().Contain(logoStatus.ToString(), "asıl Logo kodu mesajda görünmeli");
    }

    [Theory]
    [InlineData(400)]
    [InlineData(500)]
    public async Task LogoDigerHatalari_OlduguGibiAktarilir(int logoStatus)
    {
        LogoDoner(logoStatus, "Logo iş hatası");

        var result = await _sut.CreateKrediKartTahsilatAsync(Istek());

        result.StatusCode.Should().Be(logoStatus);
    }

    [Fact]
    public async Task LogoHatasi_GercekMesajiErrorsAltindaTasir()
    {
        LogoDoner(400, "FICHENO zaten kayıtlı");

        var result = await _sut.CreateKrediKartTahsilatAsync(Istek());

        result.Errors.Errors.Should().Contain(x => x.Contains("FICHENO"));
    }

    // --- Fiş numarası sınırı: Logo FICHENO varchar(17) → 16 kullanılabilir karakter ---

    [Theory]
    [InlineData("YED26073115405444E2D", false)]  // 20 karakter — musterinin gonderdigi, reddedilmeli
    [InlineData("YED260731154054", true)]        // 15 karakter
    [InlineData("YED2607311540544", true)]       // 16 karakter — tam sinir
    [InlineData(null, true)]                     // bos — Logo otomatik numara uretir
    public void FisNumarasi_16KarakterSiniriDogrulanir(string? number, bool gecerliOlmali)
    {
        var istek = Istek(number);
        var sonuclar = new List<ValidationResult>();

        var gecerli = Validator.TryValidateObject(istek, new ValidationContext(istek), sonuclar, validateAllProperties: true);

        gecerli.Should().Be(gecerliOlmali);
        if (!gecerliOlmali)
            sonuclar.Should().Contain(x => x.ErrorMessage!.Contains("16 karakter"));
    }

    [Fact]
    public async Task BasariliDurumda_LogoyaGidenTimePaketlenmisOlmali()
    {
        string json = string.Empty;
        _logo.Setup(x => x.HttpPost("ArpSlips", It.IsAny<string>()))
             .Callback<string, string>((_, j) => json = j)
             .ReturnsAsync(ResponseDto<string>.SuccessData(200, "ok", "REF"));

        await _sut.CreateKrediKartTahsilatAsync(Istek());

        var payload = JsonConvert.DeserializeObject<dynamic>(json)!;
        ((int)payload["TIME"]).Should().Be(203951616, "12:40:14 -> paketlenmis deger");
    }
}
