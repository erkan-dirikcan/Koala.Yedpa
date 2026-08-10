using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.KrediKartTahsilat;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services;
using Koala.Yedpa.Service.Tests;
using Xunit;

namespace Koala.Yedpa.Service.Tests.Services;

/// <summary>
/// Unit tests for KrediKartTahsilatService.
/// Slim model: CollectionDate, BankAccCode, Number?, Guid?, Items[].
/// - Sabitler (TYPE=70, PRINT_COUNTER=3, CURRSEL_TOTALS=1, AFFECT_RISK=1, MODULENR=5,
///   PAYMENT_TYPE=4, DISCTRDELLIST=4, TRCODE=70) serviste set edilir.
/// - BANKACC_CODE request'ten gelir; BANK_GL_CODE ve CARDREF payload'a EKLENMEZ (Logo çözer).
/// - NUMBER boşsa "~", GUID boşsa üretilir; CustomerCode'a göre gruplama.
/// </summary>
public class KrediKartTahsilatServiceTests : ServiceTestBase
{
    private readonly Mock<ILogoRestServiceProvider> _logoServiceProviderMock;
    private readonly Mock<ILogger<KrediKartTahsilatService>> _loggerMock;
    private readonly IKrediKartTahsilatService _sut;

    private const string TestBankAccCode = "11    0015";

    public KrediKartTahsilatServiceTests()
    {
        _logoServiceProviderMock = new Mock<ILogoRestServiceProvider>();
        _loggerMock = CreateLoggerMock<KrediKartTahsilatService>();
        _sut = new KrediKartTahsilatService(_logoServiceProviderMock.Object, _loggerMock.Object);
    }

    private void SetupCapture(out Func<string> getJson, string returnData = "LOGO-REF")
    {
        string capturedJson = string.Empty;
        _logoServiceProviderMock
            .Setup(x => x.HttpPost("ArpSlips", It.IsAny<string>()))
            .Callback<string, string>((url, json) => capturedJson = json)
            .ReturnsAsync(ResponseDto<string>.SuccessData(200, "Success", returnData));
        getJson = () => capturedJson;
    }

    #region Service Mapping Tests

    [Fact]
    public async Task CreateKrediKartTahsilatAsync_ShouldMapInputToLogoWithConstants()
    {
        // Arrange
        var request = new CreateKrediKartTahsilatRequestDto
        {
            CollectionDate = new DateTime(2026, 06, 24, 14, 30, 0),
            BankAccCode = TestBankAccCode,
            Notes1 = "Test tahsilat",
            Items = new List<KrediKartTahsilatItemDto>
            {
                new() { CustomerCode = "120.01.0001", Amount = 1500.00m, CustomerName = "Test Müşteri A" }
            }
        };
        SetupCapture(out var getJson, "LOGO-REF-123");

        // Act
        var result = await _sut.CreateKrediKartTahsilatAsync(request);

        // Assert - service response
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        var capturedJson = getJson();
        capturedJson.Should().NotBeNullOrEmpty();
        var parsed = JsonConvert.DeserializeObject<dynamic>(capturedJson);

        // Header - sabitler + türetmeler
        Assert.Equal(70, (int)parsed["TYPE"]);
        Assert.Equal(3, (int)parsed["PRINT_COUNTER"]);
        Assert.Equal(1, (int)parsed["CURRSEL_TOTALS"]);
        Assert.Equal("~", parsed["NUMBER"]?.ToString());                       // Number boş -> "~"
        Assert.Equal(1500.00m, (decimal)parsed["TOTAL_CREDIT"]);
        Assert.Equal(TestBankAccCode, parsed["BANKACC_CODE"]?.ToString());     // request'ten
        Assert.Equal("120.01.0001", parsed["ARP_CODE"]?.ToString());          // tek cari -> header set
        Assert.NotNull(parsed["GUID"]);                                       // üretilir

        // Transaction
        var tx = parsed["TRANSACTIONS"]["items"][0];
        Assert.Equal(1, (int)tx["AFFECT_RISK"]);
        Assert.Equal(TestBankAccCode, tx["BANKACC_CODE"]?.ToString());
        Assert.Null(tx["BANK_GL_CODE"]);                                      // gönderilmez
        Assert.NotNull(tx["GUID"]);

        // Payment
        var pay = tx["PAYMENT_LIST"]["items"][0];
        Assert.Equal(5, (int)pay["MODULENR"]);
        Assert.Equal(4, (int)pay["PAYMENT_TYPE"]);
        Assert.Equal(4, (int)pay["DISCTRDELLIST"]);
        Assert.Equal(70, (int)pay["TRCODE"]);                                 // sabit 70
        Assert.Equal(TestBankAccCode, pay["BANKACC_CODE"]?.ToString());
        Assert.Equal(1500.00m, (decimal)pay["TOTAL"]);
        Assert.Null(pay["CARDREF"]);                                          // gönderilmez (NullValueHandling.Ignore)

        _logoServiceProviderMock.Verify(x => x.HttpPost("ArpSlips", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateKrediKartTahsilatAsync_ShouldPreserveUserInputValues()
    {
        var request = new CreateKrediKartTahsilatRequestDto
        {
            CollectionDate = new DateTime(2026, 06, 24, 14, 30, 0),
            BankAccCode = TestBankAccCode,
            Notes1 = "Kullanıcı notu",
            Items = new List<KrediKartTahsilatItemDto>
            {
                new() { CustomerCode = "120.01.8888", Amount = 3000.50m, CustomerName = "Test Müşteri B" }
            }
        };
        SetupCapture(out var getJson, "LOGO-REF-456");

        var result = await _sut.CreateKrediKartTahsilatAsync(request);

        result.IsSuccess.Should().BeTrue();

        var parsed = JsonConvert.DeserializeObject<dynamic>(getJson());
        Assert.Equal("Kullanıcı notu", parsed["NOTES1"]?.ToString());
        Assert.Equal(3000.50m, (decimal)parsed["TOTAL_CREDIT"]);
        Assert.Equal("120.01.8888", parsed["ARP_CODE"]?.ToString());

        // TIME Logo'nun PAKETLENMİŞ formatındadır (HHmmss DEĞİL):
        // 14:30:00 -> 16777216*14 + 65536*30 = 236847104. Bkz. LogoTimeFormatTests.
        Assert.Equal(236847104, (int)parsed["TIME"]);
        Assert.Equal(14, (int)parsed["HOUR"]);
        Assert.Equal(30, (int)parsed["MINUTE"]);

        var tx = parsed["TRANSACTIONS"]["items"][0];
        Assert.Equal("120.01.8888", tx["ARP_CODE"]?.ToString());
        Assert.Equal(3000.50m, (decimal)tx["CREDIT"]);
    }

    [Fact]
    public async Task CreateKrediKartTahsilatAsync_ShouldUseProvidedNumberAndGuid_WhenSupplied()
    {
        var request = new CreateKrediKartTahsilatRequestDto
        {
            CollectionDate = new DateTime(2026, 06, 24),
            BankAccCode = TestBankAccCode,
            Number = "FIS-001",
            Guid = "11111111-1111-1111-1111-111111111111",
            Items = new List<KrediKartTahsilatItemDto>
            {
                new() { CustomerCode = "120.01.0001", Amount = 100.00m }
            }
        };
        SetupCapture(out var getJson);

        await _sut.CreateKrediKartTahsilatAsync(request);

        var parsed = JsonConvert.DeserializeObject<dynamic>(getJson());
        Assert.Equal("FIS-001", parsed["NUMBER"]?.ToString());                                 // müşteri verdi
        Assert.Equal("11111111-1111-1111-1111-111111111111", parsed["GUID"]?.ToString());     // müşteri verdi
    }

    [Fact]
    public async Task CreateKrediKartTahsilatAsync_ShouldGroupByCustomerCode_WhenMultipleItems()
    {
        var request = new CreateKrediKartTahsilatRequestDto
        {
            CollectionDate = new DateTime(2026, 06, 24),
            BankAccCode = TestBankAccCode,
            Items = new List<KrediKartTahsilatItemDto>
            {
                new() { CustomerCode = "120.01.0001", Amount = 1000.00m },
                new() { CustomerCode = "120.01.0001", Amount = 500.00m },
                new() { CustomerCode = "120.01.0002", Amount = 3000.00m }
            }
        };
        SetupCapture(out var getJson, "LOGO-REF-MULTI");

        var result = await _sut.CreateKrediKartTahsilatAsync(request);
        result.IsSuccess.Should().BeTrue();

        var parsed = JsonConvert.DeserializeObject<dynamic>(getJson());

        // TOTAL_CREDIT = 1000 + 500 + 3000 = 4500
        Assert.Equal(4500.00m, (decimal)parsed["TOTAL_CREDIT"]);
        // Çok cari -> header ARP_CODE null
        Assert.Null((string?)parsed["ARP_CODE"]);

        var transactions = parsed["TRANSACTIONS"]["items"] as Newtonsoft.Json.Linq.JArray;
        Assert.NotNull(transactions);
        Assert.Equal(2, transactions.Count); // 2 gruplu CustomerCode

        // İlk grup (120.01.0001): 1000 + 500 = 1500
        var first = transactions[0];
        Assert.Equal("120.01.0001", first["ARP_CODE"]?.ToString());
        Assert.Equal(1500.00m, (decimal)first["CREDIT"]);

        var firstPayments = first["PAYMENT_LIST"]["items"] as Newtonsoft.Json.Linq.JArray;
        Assert.NotNull(firstPayments);
        Assert.Equal(1, firstPayments.Count);
        Assert.Equal(1500.00m, (decimal)firstPayments[0]["TOTAL"]);

        // İkinci grup (120.01.0002): 3000
        var second = transactions[1];
        Assert.Equal("120.01.0002", second["ARP_CODE"]?.ToString());
        Assert.Equal(3000.00m, (decimal)second["CREDIT"]);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task CreateKrediKartTahsilatAsync_ShouldReturnFail_WhenLogoProviderReturnsError()
    {
        var request = CreateValidRequest();
        _logoServiceProviderMock
            .Setup(x => x.HttpPost("ArpSlips", It.IsAny<string>()))
            .ReturnsAsync(ResponseDto<string>.FailData(500, "Logo Error", "Connection failed", true));

        var result = await _sut.CreateKrediKartTahsilatAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500, "401/403 dışındaki Logo kodları olduğu gibi aktarılır");
        result.Message.Should().Contain("Logo", "hatanın kaynağı mesajda açıkça belirtilmeli");
        result.Data.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task CreateKrediKartTahsilatAsync_ShouldReturnFail_WhenExceptionThrown()
    {
        var request = CreateValidRequest();
        _logoServiceProviderMock
            .Setup(x => x.HttpPost("ArpSlips", It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Network error"));

        var result = await _sut.CreateKrediKartTahsilatAsync(request);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("bir hata oluştu");
    }

    #endregion

    #region Helper Methods

    private CreateKrediKartTahsilatRequestDto CreateValidRequest()
    {
        return new CreateKrediKartTahsilatRequestDto
        {
            CollectionDate = new DateTime(2026, 06, 24, 12, 0, 0),
            BankAccCode = TestBankAccCode,
            Notes1 = "Test fiş",
            Items = new List<KrediKartTahsilatItemDto>
            {
                new() { CustomerCode = "120.01.0001", Amount = 1000.00m, CustomerName = "Test Müşteri" }
            }
        };
    }

    #endregion
}
