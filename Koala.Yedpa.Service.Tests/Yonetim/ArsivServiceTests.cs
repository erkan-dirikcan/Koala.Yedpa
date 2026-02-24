using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services.Yonetim;
using Koala.Yedpa.Service.Tests;
using Xunit;

namespace Koala.Yedpa.Service.Tests.Yonetim;

/// <summary>
/// Unit tests for ArsivService
/// </summary>
public class ArsivServiceTests
{
    private readonly Mock<IArsivRepository> _repositoryMock;
    private readonly Mock<ILogger<ArsivService>> _loggerMock;
    private readonly IArsivService _sut;

    public ArsivServiceTests()
    {
        _repositoryMock = new Mock<IArsivRepository>();
        _loggerMock = new Mock<ILogger<ArsivService>>();
        _sut = new ArsivService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetArsivListesiAsync_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var expectedData = new List<ArsivDto>
        {
            new ArsivDto { RafID = 1, RafKod = "RAF01", BolmeNo = "B1", KoliNo = "K1" },
            new ArsivDto { RafID = 2, RafKod = "RAF02", BolmeNo = "B2", KoliNo = "K2" }
        };
        _repositoryMock.Setup(r => r.GetArsivListesiAsync()).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetArsivListesiAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(2);
        result.Data.First().RafKod.Should().Be("RAF01");
        _repositoryMock.Verify(r => r.GetArsivListesiAsync(), Times.Once);
    }

    [Fact]
    public async Task GetArsivListesiAsync_ShouldReturnFail_WhenExceptionThrown()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetArsivListesiAsync())
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.GetArsivListesiAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("başarısız");
    }

    [Fact]
    public async Task GetKoliDetayAsync_ShouldReturnSuccess_WhenKoliExists()
    {
        // Arrange
        var expectedData = new ArsivDetayDto
        {
            RafID = 1,
            RafKod = "RAF01",
            BolmeID = 1,
            BolmeNo = "B1",
            KoliID = 1,
            KoliNo = "K1",
            Detay = "Test Detay",
            Icerik = "İçerik",
            ToplamEsya = 5
        };
        _repositoryMock.Setup(r => r.GetKoliDetayAsync(1)).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetKoliDetayAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.KoliNo.Should().Be("K1");
        result.Data.Detay.Should().Be("Test Detay");
    }

    [Fact]
    public async Task GetKoliDetayAsync_ShouldReturnFail_WhenKoliNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetKoliDetayAsync(1)).ReturnsAsync((ArsivDetayDto?)null);

        // Act
        var result = await _sut.GetKoliDetayAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task AddRafAsync_ShouldReturnSuccess_WhenRafAdded()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AddRafAsync(It.IsAny<Raf>())).ReturnsAsync(true);

        // Act
        var result = await _sut.AddRafAsync("RAF_NEW");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("başarıyla eklendi");
        result.Data.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddRafAsync(It.Is<Raf>(raf => raf.RafKod == "RAF_NEW")), Times.Once);
    }

    [Fact]
    public async Task AddRafAsync_ShouldReturnFail_WhenExceptionThrown()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AddRafAsync(It.IsAny<Raf>()))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _sut.AddRafAsync("RAF_NEW");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task AddBolmeAsync_ShouldReturnSuccess_WhenBolmeAdded()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AddBolmeAsync(It.IsAny<Bolme>())).ReturnsAsync(true);

        // Act
        var result = await _sut.AddBolmeAsync(1, "B_NEW");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddBolmeAsync(It.Is<Bolme>(b => b.RafID == 1 && b.BolmeNo == "B_NEW")), Times.Once);
    }

    [Fact]
    public async Task AddKoliAsync_ShouldReturnSuccess_WhenKoliAdded()
    {
        // Arrange
        _repositoryMock.Setup(r => r.AddKoliAsync(It.IsAny<Koli>())).ReturnsAsync(true);

        // Act
        var result = await _sut.AddKoliAsync(1, "K_NEW", "Test Detay");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddKoliAsync(It.Is<Koli>(k => k.BolmeID == 1 && k.KoliNo == "K_NEW" && k.Detay == "Test Detay")), Times.Once);
    }

    [Fact]
    public async Task UpdateKoliAsync_ShouldReturnSuccess_WhenKoliUpdated()
    {
        // Arrange
        _repositoryMock.Setup(r => r.UpdateKoliAsync(It.IsAny<Koli>())).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateKoliAsync(1, "K_UPD", "Güncellenmiş Detay");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateKoliAsync(It.Is<Koli>(k => k.KoliID == 1 && k.KoliNo == "K_UPD")), Times.Once);
    }

    [Fact]
    public async Task UpdateKoliAsync_ShouldReturnFail_WhenKoliNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.UpdateKoliAsync(It.IsAny<Koli>())).ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateKoliAsync(999, "K_UPD", "Detay");

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("bulunamadı");
    }

    [Fact]
    public async Task DeleteKoliAsync_ShouldReturnSuccess_WhenKoliDeleted()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteKoliAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteKoliAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteKoliAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteKoliAsync_ShouldReturnFail_WhenKoliNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteKoliAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteKoliAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
