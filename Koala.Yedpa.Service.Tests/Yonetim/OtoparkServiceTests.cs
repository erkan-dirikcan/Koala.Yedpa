using Koala.Yedpa.Core.Models.Yonetim;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services.Yonetim;
using Koala.Yedpa.Service.Tests;
using Xunit;

namespace Koala.Yedpa.Service.Tests.Yonetim;

/// <summary>
/// Unit tests for OtoparkService
/// </summary>
public class OtoparkServiceTests
{
    private readonly Mock<IOtoparkRepository> _repositoryMock;
    private readonly Mock<ILogger<OtoparkService>> _loggerMock;
    private readonly IOtoparkService _sut;

    public OtoparkServiceTests()
    {
        _repositoryMock = new Mock<IOtoparkRepository>();
        _loggerMock = new Mock<ILogger<OtoparkService>>();
        _sut = new OtoparkService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var expectedData = new List<OtoparkListDto>
        {
            new OtoparkListDto { KayitID = 1, Plaka = "34ABC123", AboneAd = "Abone 1" },
            new OtoparkListDto { KayitID = 2, Plaka = "34XYZ456", AboneAd = "Abone 2" }
        };
        _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetActiveAsync_ShouldReturnActiveSubscriptions()
    {
        // Arrange
        var expectedData = new List<OtoparkListDto>
        {
            new OtoparkListDto { KayitID = 1, Plaka = "34ACT123", CikisTarih = null },
            new OtoparkListDto { KayitID = 2, Plaka = "34ACT456", CikisTarih = null }
        };
        _repositoryMock.Setup(r => r.GetActiveSubscriptionsAsync()).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetActiveAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data.All(d => d.Aktif).Should().BeTrue();
        _repositoryMock.Verify(r => r.GetActiveSubscriptionsAsync(), Times.Once);
    }

    [Fact]
    public async Task GetByPlakaAsync_ShouldReturnSuccess_WhenKayitExists()
    {
        // Arrange
        var kayit = new OtoparkKayit
        {
            KayitID = 1,
            Plaka = "34TEST12",
            GirisTarih = DateTime.Now,
            CikisTarih = null,
            AboneAd = "Test Abone"
        };
        _repositoryMock.Setup(r => r.GetByPlakaAsync("34TEST12")).ReturnsAsync(kayit);

        // Act
        var result = await _sut.GetByPlakaAsync("34TEST12");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Plaka.Should().Be("34TEST12");
        result.Data.AboneAd.Should().Be("Test Abone");
    }

    [Fact]
    public async Task GetByPlakaAsync_ShouldReturnFail_WhenKayitNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByPlakaAsync("34NOTFND")).ReturnsAsync((OtoparkKayit?)null);

        // Act
        var result = await _sut.GetByPlakaAsync("34NOTFND");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GirisYapAsync_ShouldReturnSuccess_WhenVehicleNotInside()
    {
        // Arrange
        var dto = new OtoparkGirisDto
        {
            Plaka = "34IN999",
            AboneAd = "Test Abone",
            Telefon = "5551234567"
        };
        _repositoryMock.Setup(r => r.GetByPlakaAsync("34IN999")).ReturnsAsync((OtoparkKayit?)null);
        _repositoryMock.Setup(r => r.GirisYapAsync(It.IsAny<OtoparkKayit>())).ReturnsAsync(true);

        // Act
        var result = await _sut.GirisYapAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        _repositoryMock.Verify(r => r.GirisYapAsync(It.IsAny<OtoparkKayit>()), Times.Once);
    }

    [Fact]
    public async Task GirisYapAsync_ShouldReturnFail_WhenVehicleAlreadyInside()
    {
        // Arrange
        var dto = new OtoparkGirisDto { Plaka = "34IN888" };
        var existingKayit = new OtoparkKayit
        {
            KayitID = 1,
            Plaka = "34IN888",
            GirisTarih = DateTime.Now,
            CikisTarih = null
        };
        _repositoryMock.Setup(r => r.GetByPlakaAsync("34IN888")).ReturnsAsync(existingKayit);

        // Act
        var result = await _sut.GirisYapAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        result.Message.Should().Contain("zaten içinde");
        _repositoryMock.Verify(r => r.GirisYapAsync(It.IsAny<OtoparkKayit>()), Times.Never);
    }

    [Fact]
    public async Task CikisYapAsync_ShouldReturnSuccess_WhenVehicleInside()
    {
        // Arrange
        var dto = new OtoparkCikisDto { Plaka = "34OUT777" };
        _repositoryMock.Setup(r => r.CikisYapAsync("34OUT777")).ReturnsAsync(true);

        // Act
        var result = await _sut.CikisYapAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        _repositoryMock.Verify(r => r.CikisYapAsync("34OUT777"), Times.Once);
    }

    [Fact]
    public async Task CikisYapAsync_ShouldReturnFail_WhenVehicleNotInside()
    {
        // Arrange
        var dto = new OtoparkCikisDto { Plaka = "34OUT666" };
        _repositoryMock.Setup(r => r.CikisYapAsync("34OUT666")).ReturnsAsync(false);

        // Act
        var result = await _sut.CikisYapAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AboneEkleAsync_ShouldReturnSuccess_WhenAboneCreated()
    {
        // Arrange
        var dto = new OtoparkAboneDto
        {
            Plaka = "34SUB111",
            AboneAd = "Abone User",
            Telefon = "5555555555",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddMonths(1)
        };
        _repositoryMock.Setup(r => r.AboneEkleAsync(It.IsAny<OtoparkKayit>())).ReturnsAsync(true);

        // Act
        var result = await _sut.AboneEkleAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        _repositoryMock.Verify(r => r.AboneEkleAsync(It.IsAny<OtoparkKayit>()), Times.Once);
    }

    [Fact]
    public async Task AboneGuncelleAsync_ShouldReturnSuccess_WhenAboneExists()
    {
        // Arrange
        var dto = new OtoparkAboneDto
        {
            Plaka = "34SUB222",
            AboneAd = "Güncellenen Abone",
            Telefon = "5321111111",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddMonths(2)
        };
        var existingKayit = new OtoparkKayit
        {
            KayitID = 1,
            Plaka = "34SUB222",
            AboneAd = "Eski Abone",
            Telefon = "5551111111"
        };
        _repositoryMock.Setup(r => r.GetByPlakaAsync("34SUB222")).ReturnsAsync(existingKayit);
        _repositoryMock.Setup(r => r.AboneGuncelleAsync(It.IsAny<OtoparkKayit>())).ReturnsAsync(true);

        // Act
        var result = await _sut.AboneGuncelleAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.AboneGuncelleAsync(It.IsAny<OtoparkKayit>()), Times.Once);
    }

    [Fact]
    public async Task AboneGuncelleAsync_ShouldReturnFail_WhenAboneNotFound()
    {
        // Arrange
        var dto = new OtoparkAboneDto
        {
            Plaka = "34NOTFND",
            AboneAd = "Test",
            Telefon = "5551234567",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddMonths(1)
        };
        _repositoryMock.Setup(r => r.GetByPlakaAsync("34NOTFND")).ReturnsAsync((OtoparkKayit?)null);

        // Act
        var result = await _sut.AboneGuncelleAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AboneSilAsync_ShouldReturnSuccess_WhenAboneDeleted()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AboneSilAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.AboneSilAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.AboneSilAsync(1), Times.Once);
    }

    [Fact]
    public async Task AboneSilAsync_ShouldReturnFail_WhenAboneNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AboneSilAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.AboneSilAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
