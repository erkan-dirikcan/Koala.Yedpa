using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Service.Services.Yonetim;
using Koala.Yedpa.Service.Tests;
using Xunit;

namespace Koala.Yedpa.Service.Tests.Yonetim;

/// <summary>
/// Unit tests for OrtakService
/// </summary>
public class OrtakServiceTests
{
    private readonly Mock<IOrtakRepository> _repositoryMock;
    private readonly Mock<ILogger<OrtakService>> _loggerMock;
    private readonly IOrtakService _sut;

    public OrtakServiceTests()
    {
        _repositoryMock = new Mock<IOrtakRepository>();
        _loggerMock = new Mock<ILogger<OrtakService>>();
        _sut = new OrtakService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region Birim Tests

    [Fact]
    public async Task GetAllBirimlerAsync_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var expectedData = new List<Birim>
        {
            new Birim { BirimID = 1, BirimAdi = "IT" },
            new Birim { BirimID = 2, BirimAdi = "HR" },
            new Birim { BirimID = 3, BirimAdi = "Finance" }
        };
        _repositoryMock.Setup(r => r.GetAllBirimlerAsync()).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetAllBirimlerAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(3);
        _repositoryMock.Verify(r => r.GetAllBirimlerAsync(), Times.Once);
    }

    #endregion

    #region Mail Tests

    [Fact]
    public async Task GetAllMailAdresleriAsync_ShouldReturnSuccess_WhenDataExists()
    {
        // Arrange
        var expectedData = new List<Mail>
        {
            new Mail { MailID = 1, Ad = "Ahmet", Soyad = "Demir", EPosta = "ahmet@test.com" },
            new Mail { MailID = 2, Ad = "Mehmet", Soyad = "Kaya", EPosta = "mehmet@test.com" }
        };
        _repositoryMock.Setup(r => r.GetAllMailAdresleriAsync()).ReturnsAsync(expectedData);

        // Act
        var result = await _sut.GetAllMailAdresleriAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMailByIdAsync_ShouldReturnSuccess_WhenMailExists()
    {
        // Arrange
        var mail = new Mail
        {
            MailID = 1,
            Ad = "Test",
            Soyad = "User",
            EPosta = "test@test.com",
            GSM = "5551234567",
            Telefon = "5559876543"
        };
        _repositoryMock.Setup(r => r.GetMailByIdAsync(1)).ReturnsAsync(mail);

        // Act
        var result = await _sut.GetMailByIdAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Data.Ad.Should().Be("Test");
        result.Data.EPosta.Should().Be("test@test.com");
    }

    [Fact]
    public async Task GetMailByIdAsync_ShouldReturnFail_WhenMailNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.GetMailByIdAsync(999)).ReturnsAsync((Mail?)null);

        // Act
        var result = await _sut.GetMailByIdAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task AddMailAsync_ShouldReturnSuccess_WhenMailAdded()
    {
        // Arrange
        var mail = new Mail
        {
            Ad = "Yeni",
            Soyad = "Kullanıcı",
            EPosta = "yeni@test.com",
            GSM = "5555555555",
            Telefon = "5554444444"
        };
        _repositoryMock.Setup(r => r.AddMailAsync(mail)).ReturnsAsync(true);

        // Act
        var result = await _sut.AddMailAsync(mail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        _repositoryMock.Verify(r => r.AddMailAsync(mail), Times.Once);
    }

    [Fact]
    public async Task UpdateMailAsync_ShouldReturnSuccess_WhenMailUpdated()
    {
        // Arrange
        var mail = new Mail
        {
            MailID = 1,
            Ad = "Güncellenen",
            Soyad = "User",
            EPosta = "guncel@test.com",
            GSM = "5321111111"
        };
        _repositoryMock.Setup(r => r.UpdateMailAsync(mail)).ReturnsAsync(true);

        // Act
        var result = await _sut.UpdateMailAsync(mail);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateMailAsync(mail), Times.Once);
    }

    [Fact]
    public async Task UpdateMailAsync_ShouldReturnFail_WhenMailNotFound()
    {
        // Arrange
        var mail = new Mail
        {
            MailID = 999,
            Ad = "Bulunamayan",
            Soyad = "User",
            EPosta = "notfound@test.com"
        };
        _repositoryMock.Setup(r => r.UpdateMailAsync(mail)).ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateMailAsync(mail);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteMailAsync_ShouldReturnSuccess_WhenMailDeleted()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteMailAsync(1)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteMailAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _repositoryMock.Verify(r => r.DeleteMailAsync(1), Times.Once);
    }

    [Fact]
    public async Task DeleteMailAsync_ShouldReturnFail_WhenMailNotFound()
    {
        // Arrange
        _repositoryMock.Setup(r => r.DeleteMailAsync(999)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteMailAsync(999);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    #endregion
}
