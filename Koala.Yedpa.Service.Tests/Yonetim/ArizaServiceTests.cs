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
/// Unit tests for ArizaService
/// </summary>
public class ArizaServiceTests
{
    private readonly Mock<IArizaRepository> _repositoryMock;
    private readonly Mock<ILogger<ArizaService>> _loggerMock;
    private readonly IArizaService _sut;

    public ArizaServiceTests()
    {
        _repositoryMock = new Mock<IArizaRepository>();
        _loggerMock = new Mock<ILogger<ArizaService>>();
        _sut = new ArizaService(_repositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var expectedData = new List<ArizaListDto>
        {
            new ArizaListDto { ArizaID = 1, Konu = "Arıza A", Birim = "IT", Durum = "Beklemede" },
            new ArizaListDto { ArizaID = 2, Konu = "Arıza B", Birim = "HR", Durum = "Devam Ediyor" }
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
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenArizaExists()
    {
        // Arrange
        var ariza = new Ariza
        {
            ArizaID = 1,
            FirmaAdres = "Test Firma",
            Konu = "Test Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Hareketler = new List<ArizaHareket>(),
            IlgiliKisiler = new List<ArizaKisi>()
        };
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(ariza);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Konu.Should().Be("Test Arıza");
        result.Data.Birim.Should().Be("IT");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFail_WhenArizaNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Ariza?)null);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByBirimAsync_ShouldReturnArizalarForBirim()
    {
        // Arrange
        var expectedData = new List<ArizaListDto>
        {
            new ArizaListDto { ArizaID = 1, Konu = "IT Arıza", Birim = "IT" }
        };
        _repositoryMock.Setup(r => r.GetByBirimAsync("IT")).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetByBirimAsync("IT");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.First().Birim.Should().Be("IT");
        _repositoryMock.Verify(r => r.GetByBirimAsync("IT"), Times.Once);
    }

    [Fact]
    public async Task GetActiveFaultsAsync_ShouldReturnActiveArizalar()
    {
        // Arrange
        var expectedData = new List<ArizaListDto>
        {
            new ArizaListDto { ArizaID = 1, Konu = "Aktif Arıza", Durum = "Devam Ediyor" }
        };
        _repositoryMock.Setup(r => r.GetActiveFaultsAsync()).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetActiveFaultsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        _repositoryMock.Verify(r => r.GetActiveFaultsAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenArizaCreated()
    {
        // Arrange
        var dto = new ArizaCreateDto
        {
            FirmaAdres = "Yeni Firma",
            Konu = "Yeni Arıza",
            Birim = "IT",
            Aciklama = "Açıklama",
            IlgiliKisiIds = new List<int>()
        };
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Ariza>())).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddHareketAsync(It.IsAny<ArizaHareket>())).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddIlgiliKisiAsync(It.IsAny<ArizaKisi>())).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Konu.Should().Be("Yeni Arıza");
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<Ariza>()), Times.Once);
        _repositoryMock.Verify(r => r.AddHareketAsync(It.IsAny<ArizaHareket>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldNotAddHareket_WhenAciklamaEmpty()
    {
        // Arrange
        var dto = new ArizaCreateDto
        {
            FirmaAdres = "Yeni Firma",
            Konu = "Yeni Arıza",
            Birim = "IT",
            Aciklama = "",
            IlgiliKisiIds = new List<int>()
        };
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Ariza>())).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddHareketAsync(It.IsAny<ArizaHareket>()), Times.Never);
    }

    [Fact]
    public async Task UpdateDurumAsync_ShouldReturnSuccess_WhenDurumUpdated()
    {
        // Arrange
        var dto = new ArizaDurumUpdateDto
        {
            ArizaID = 1,
            Durum = "Tamamlandı",
            SonKisi = "Test User"
        };
        _repositoryMock.Setup(r => r.UpdateDurumAsync(1, "Tamamlandı", "Test User")).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddHareketAsync(It.IsAny<ArizaHareket>())).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateDurumAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateDurumAsync(1, "Tamamlandı", "Test User"), Times.Once);
        _repositoryMock.Verify(r => r.AddHareketAsync(It.Is<ArizaHareket>(h => h.Aciklama != null && h.Aciklama.Contains("Durum değişti"))), Times.Once);
    }

    [Fact]
    public async Task UpdateDurumAsync_ShouldReturnFail_WhenArizaNotFound()
    {
        // Arrange
        var dto = new ArizaDurumUpdateDto
        {
            ArizaID = 999,
            Durum = "Tamamlandı",
            SonKisi = "Test"
        };
        _repositoryMock.Setup(r => r.UpdateDurumAsync(999, "Tamamlandı", "Test")).ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateDurumAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddHareketAsync_ShouldReturnSuccess_WhenHareketAdded()
    {
        // Arrange
        var dto = new ArizaHareketEkleDto
        {
            ArizaID = 1,
            Aciklama = "Test hareketi",
            Kisi = "Admin"
        };
        _repositoryMock.Setup(r => r.AddHareketAsync(It.IsAny<ArizaHareket>())).ReturnsAsync(true);

        // Act
        var result = await _sut.AddHareketAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddHareketAsync(It.Is<ArizaHareket>(h => h.ArizaID == 1 && h.Aciklama == "Test hareketi")), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenArizaDeleted()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFail_WhenArizaNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddIlgiliKisiler_WhenProvided()
    {
        // Arrange
        var dto = new ArizaCreateDto
        {
            FirmaAdres = "Test Firma",
            Konu = "Test Arıza",
            Birim = "IT",
            Aciklama = "Açıklama",
            IlgiliKisiIds = new List<int> { 1, 2 }
        };
        _repositoryMock.Setup(r => r.CreateAsync(It.IsAny<Ariza>())).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.AddIlgiliKisiAsync(It.IsAny<ArizaKisi>())).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.AddIlgiliKisiAsync(It.IsAny<ArizaKisi>()), Times.Exactly(2));
    }
}
