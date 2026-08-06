using System.Text;
using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.KrediKartTahsilat;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services;
using Moq;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Koala.Yedpa.Service.Tests.Services;

/// <summary>
/// Teşhis testi — müşterinin (Yedpa-Web) 01.08.2026'da 401 aldığı GERÇEK istek gövdesini
/// servisten geçirip Logo ArpSlips'e giden JSON'u üretir.
///
/// Logo'ya çağrı YAPILMAZ (provider mock'lanmıştır), fiş OLUŞMAZ.
/// Kaynak: Documents/yedpa.txt — müşteri WhatsApp mesajı, 13:19 01.08.2026.
/// </summary>
public class KrediKartTahsilatPayloadDumpTests : ServiceTestBase
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogoRestServiceProvider> _logoProviderMock = new();
    private readonly IKrediKartTahsilatService _sut;

    public KrediKartTahsilatPayloadDumpTests(ITestOutputHelper output)
    {
        _output = output;
        _sut = new KrediKartTahsilatService(
            _logoProviderMock.Object,
            CreateLoggerMock<KrediKartTahsilatService>().Object);
    }

    [Fact]
    public async Task MusteriIstegi_LogoyaGidenJsonuUret()
    {
        // Müşterinin gönderdiği gövdenin birebir kopyası:
        // {
        //   "collectionDate": "2026-07-31T12:40:14",
        //   "bankAccCode": "02    0002",
        //   "items": [ { "customerCode": "1.G000.254.AS.K4", "amount": 2535 } ],
        //   "number": "YED26073115405444E2D",
        //   "guid": "3f2a9c14-5b7e-4d21-9a08-6c1e5b3f7d42",
        //   "notes1": "Sanal POS (yapikredi) YED2026000010511"
        // }
        var request = new CreateKrediKartTahsilatRequestDto
        {
            CollectionDate = new DateTime(2026, 07, 31, 12, 40, 14),
            BankAccCode = "02    0002",
            Number = "YED26073115405444E2D",
            Guid = "3f2a9c14-5b7e-4d21-9a08-6c1e5b3f7d42",
            Notes1 = "Sanal POS (yapikredi) YED2026000010511",
            Items = new List<KrediKartTahsilatItemDto>
            {
                new() { CustomerCode = "1.G000.254.AS.K4", Amount = 2535m }
            }
        };

        string gonderilenJson = string.Empty;
        _logoProviderMock
            .Setup(x => x.HttpPost("ArpSlips", It.IsAny<string>()))
            .Callback<string, string>((_, json) => gonderilenJson = json)
            .ReturnsAsync(ResponseDto<string>.SuccessData(200, "Success", "ORNEK-REF"));

        var result = await _sut.CreateKrediKartTahsilatAsync(request);

        result.IsSuccess.Should().BeTrue();
        gonderilenJson.Should().NotBeNullOrEmpty();

        var guzel = JsonConvert.SerializeObject(
            JsonConvert.DeserializeObject(gonderilenJson), Formatting.Indented);

        var dosya = Path.Combine(AppContext.BaseDirectory, "kredikart-ornek-payload.json");
        File.WriteAllText(dosya, guzel, new UTF8Encoding(false));

        _output.WriteLine("=== Logo ArpSlips'e giden JSON ===");
        _output.WriteLine(guzel);
        _output.WriteLine("");
        _output.WriteLine($"Dosyaya yazildi: {dosya}");
    }
}
