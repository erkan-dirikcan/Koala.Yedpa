using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models.ViewModels;
using Koala.Yedpa.Core.Services;
using Koala.Yedpa.Core.Providers;
using Koala.Yedpa.Service.Providers;
using System.Net;
using System.Net.Http;
using Xunit;

namespace Koala.Yedpa.Service.Tests;

/// <summary>
/// Unit tests for LogoRestServiceProvider.PingAsync()
/// Coverage Target: %90+
/// </summary>
public class LogoRestServiceProviderTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<ISettingsService> _settingsServiceMock;
    private readonly Mock<ILicenseReader> _licenseReaderMock;
    private readonly Mock<ILogger<LogoRestServiceProvider>> _loggerMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly LogoRestServiceProvider _sut;

    public LogoRestServiceProviderTests()
    {
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _settingsServiceMock = new Mock<ISettingsService>();
        _licenseReaderMock = new Mock<ILicenseReader>();
        _loggerMock = new Mock<ILogger<LogoRestServiceProvider>>();
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost")
        };

        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        _sut = new LogoRestServiceProvider(
            _httpClientFactoryMock.Object,
            _settingsServiceMock.Object,
            _licenseReaderMock.Object,
            _loggerMock.Object);
    }

    private LogoRestServiceSettingViewModel CreateValidSettings(
        string server = "http://logo-server.example.com",
        int port = 8080,
        string userName = "test-user",
        string password = "test-pass",
        string firm = "001",
        string period = "2024")
    {
        return new LogoRestServiceSettingViewModel
        {
            Server = server,
            Port = port,
            UserName = userName,
            Password = password,
            Firm = firm,
            Period = period
        };
    }

    [Fact]
    public async Task PingAsync_WhenCalledWithValidSettings_ReturnsPong()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        var expectedResponse = "\"pong\"";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse)
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("başarılı");
        result.Data.Should().Be(expectedResponse);

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenSettingsNotFound_ReturnsFail()
    {
        // Arrange
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.FailData(404, "Ayarlar bulunamadı", "Logo REST ayarları bulunamadı", true));

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("Ayarlar alınamadı");
        result.Errors.Errors.First().Should().Contain("Logo REST ayarları bulunamadı");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PingAsync_WhenSettingsReturnNullData_ReturnsFail()
    {
        // Arrange
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", null!));

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("Ayarlar alınamadı");
        result.Errors.Errors.First().Should().Contain("Logo REST ayarları bulunamadı");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task PingAsync_WhenHttpRequestExceptionThrown_ReturnsFail()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("Ping bağlantı hatası");
        result.Errors.Errors.First().Should().Contain("Connection refused");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenWebExceptionThrown_ReturnsFail()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        var webEx = new WebException("Server error", WebExceptionStatus.ConnectFailure);

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(webEx);

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("Ping web hatası");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenGeneralExceptionThrown_ReturnsFail()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("Ping hatası");
        result.Errors.Errors.First().Should().Contain("Unexpected error");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerReturns404_ReturnsFail()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Endpoint not found")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        result.Message.Should().Contain("Ping başarısız");
        result.Errors.Errors.First().Should().Contain("Endpoint not found");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerReturns500_ReturnsFail()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Internal Server Error")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(500);
        result.Message.Should().Contain("Ping başarısız");
        result.Errors.Errors.First().Should().Contain("Internal Server Error");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerReturns401Unauthorized_ReturnsFail()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent("Unauthorized")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
        result.Message.Should().Contain("Ping başarısız");
        result.Errors.Errors.First().Should().Contain("Unauthorized");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerReturns503ServiceUnavailable_ReturnsFail()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable,
                Content = new StringContent("Service Unavailable")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(503);
        result.Message.Should().Contain("Ping başarısız");
        result.Errors.Errors.First().Should().Contain("Service Unavailable");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerReturnsCustomPongMessage_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        var customResponse = "{\"status\":\"pong\",\"version\":\"1.0\"}";
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(customResponse)
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("Ping başarılı");
        result.Data.Should().Be(customResponse);

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerReturnsEmptyString_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);
        result.Message.Should().Contain("Ping başarılı");
        result.Data.Should().Be("");

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerHasDifferentPort_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateValidSettings(port: 9090);
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("\"pong\"")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_WhenServerHasHttpsUrl_ReturnsSuccess()
    {
        // Arrange
        var settings = CreateValidSettings(server: "https://logo-server.example.com");
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("\"pong\"")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(200);

        _settingsServiceMock.Verify(x => x.GetLogoRestServiceSettingsAsync(), Times.Once);
        _httpClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task PingAsync_ShouldLogInformationOnSuccess()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("\"pong\"")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logo REST API ping atılıyor")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logo REST API ping başarılı")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PingAsync_ShouldLogErrorOnHttpRequestException()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("PingAsync HTTP hatası")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PingAsync_ShouldLogWarningOnNonSuccessStatusCode()
    {
        // Arrange
        var settings = CreateValidSettings();
        _settingsServiceMock.Setup(x => x.GetLogoRestServiceSettingsAsync())
            .ReturnsAsync(ResponseDto<LogoRestServiceSettingViewModel>.SuccessData(200, "Ayarlar alındı", settings));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri != null && req.RequestUri.PathAndQuery.Contains("/ping")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server Error")
            });

        // Act
        var result = await _sut.PingAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Logo REST API ping başarısız")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}