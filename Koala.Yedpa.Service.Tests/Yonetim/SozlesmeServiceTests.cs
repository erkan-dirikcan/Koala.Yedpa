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
/// Unit tests for SozlesmeService
/// </summary>
public class SozlesmeServiceTests
{
    private readonly Mock<ISozlesmeRepository> _sozlesmeRepositoryMock;
    private readonly Mock<IOrtakRepository> _ortakRepositoryMock;
    private readonly Mock<ILogger<SozlesmeService>> _loggerMock;
    private readonly ISozlesmeService _sut;

    public SozlesmeServiceTests()
    {
        _sozlesmeRepositoryMock = new Mock<ISozlesmeRepository>();
        _ortakRepositoryMock = new Mock<IOrtakRepository>();
        _loggerMock = new Mock<ILogger<SozlesmeService>>();
        _sut = new SozlesmeService(_sozlesmeRepositoryMock.Object, _ortakRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var expectedData = new List<SozlesmeListDto>
        {
            new SozlesmeListDto { SozlesmeID = 1, Firma = "Firma A", Konu = "Konu A" },
            new SozlesmeListDto { SozlesmeID = 2, Firma = "Firma B", Konu = "Konu B" }
        };
        _sozlesmeRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSuccess_WhenSozlesmeExists()
    {
        // Arrange
        var sozlesme = new Sozlesme
        {
            SozlesmeID = 1,
            Firma = "Test Firma",
            Konu = "Test Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            IlgiliKisiler = new List<SozlesmeKisi>()
        };
        _sozlesmeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(sozlesme);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Firma.Should().Be("Test Firma");
        result.Data.Konu.Should().Be("Test Konu");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnFail_WhenSozlesmeNotFound()
    {
        // Arrange
        _sozlesmeRepositoryMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Sozlesme?)null);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetExpiringAsync_ShouldReturnExpiringContracts()
    {
        // Arrange
        var expectedData = new List<SozlesmeListDto>
        {
            new SozlesmeListDto { SozlesmeID = 1, Firma = "Expiring Firma", KalanGun = 15 }
        };
        _sozlesmeRepositoryMock.Setup(r => r.GetExpiringContractsAsync(30)).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetExpiringAsync(30);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().HaveCount(1);
        result.Data.First().KalanGun.Should().Be(15);
        _sozlesmeRepositoryMock.Verify(r => r.GetExpiringContractsAsync(30), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldReturnSuccess_WhenSozlesmeCreated()
    {
        // Arrange
        var dto = new SozlesmeCreateDto
        {
            Firma = "Yeni Firma",
            Konu = "Yeni Konu",
            Tur = "Hizmet",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Birim = "IT",
            IlgiliKisiIds = new List<int>(),
            Pdf = null
        };
        _sozlesmeRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Sozlesme>())).ReturnsAsync(true);
        _sozlesmeRepositoryMock.Setup(r => r.AddIlgiliKisiAsync(It.IsAny<SozlesmeKisi>())).ReturnsAsync(true);
        _sozlesmeRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Sozlesme>())).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Data.Firma.Should().Be("Yeni Firma");
        _sozlesmeRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<Sozlesme>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldAddPdf_WhenProvided()
    {
        // Arrange
        var dto = new SozlesmeCreateDto
        {
            Firma = "PDF Firma",
            Konu = "PDF Konu",
            Tur = "Lisans",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Birim = "IT",
            IlgiliKisiIds = new List<int>(),
            Pdf = new byte[] { 0x25, 0x50, 0x44, 0x46 }
        };
        _sozlesmeRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Sozlesme>())).ReturnsAsync(true);
        _sozlesmeRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Sozlesme>())).ReturnsAsync(true);

        // Act
        var result = await _sut.CreateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _sozlesmeRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Sozlesme>(s => s.Pdf != null)), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnSuccess_WhenSozlesmeUpdated()
    {
        // Arrange
        var dto = new SozlesmeUpdateDto
        {
            SozlesmeID = 1,
            Firma = "Güncellenen Firma",
            Konu = "Güncellenen Konu",
            Tur = "Hizmet",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Birim = "IT",
            Pdf = null
        };
        _sozlesmeRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Sozlesme>())).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Firma.Should().Be("Güncellenen Firma");
        _sozlesmeRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Sozlesme>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFail_WhenSozlesmeNotFound()
    {
        // Arrange
        var dto = new SozlesmeUpdateDto
        {
            SozlesmeID = 999,
            Firma = "Test",
            Konu = "Test",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1)
        };
        _sozlesmeRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Sozlesme>())).ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnSuccess_WhenSozlesmeDeleted()
    {
        // Arrange
        _sozlesmeRepositoryMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        _sozlesmeRepositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFail_WhenSozlesmeNotFound()
    {
        // Arrange
        _sozlesmeRepositoryMock.Setup(r => r.DeleteAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetPdfAsync_ShouldReturnPdf_WhenExists()
    {
        // Arrange
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 };
        _sozlesmeRepositoryMock.Setup(r => r.GetContractPdfAsync(1)).ReturnsAsync(pdfBytes);

        // Act
        var result = await _sut.GetPdfAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().BeEquivalentTo(pdfBytes);
    }

    [Fact]
    public async Task GetPdfAsync_ShouldReturnFail_WhenPdfNotFound()
    {
        // Arrange
        _sozlesmeRepositoryMock.Setup(r => r.GetContractPdfAsync(1)).ReturnsAsync((byte[]?)null);

        // Act
        var result = await _sut.GetPdfAsync(1);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateDurumAsync_ShouldReturnSuccess_WhenDurumUpdated()
    {
        // Arrange
        var dto = new SozlesmeDurumUpdateDto
        {
            SozlesmeID = 1,
            Bitti = true,
            SonKisi = "Test User"
        };
        _sozlesmeRepositoryMock.Setup(r => r.UpdateContractStatusAsync(1, true, "Test User")).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateDurumAsync(dto);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _sozlesmeRepositoryMock.Verify(r => r.UpdateContractStatusAsync(1, true, "Test User"), Times.Once);
    }

    [Fact]
    public async Task UpdateDurumAsync_ShouldReturnFail_WhenSozlesmeNotFound()
    {
        // Arrange
        var dto = new SozlesmeDurumUpdateDto
        {
            SozlesmeID = 999,
            Bitti = false,
            SonKisi = "Test"
        };
        _sozlesmeRepositoryMock.Setup(r => r.UpdateContractStatusAsync(999, false, "Test")).ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateDurumAsync(dto);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
