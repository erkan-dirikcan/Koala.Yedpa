using FluentAssertions;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Service.Providers;
using Xunit;

namespace Koala.Yedpa.Service.Tests.Providers
{
    /// <summary>
    /// LogoRestServiceProvider token alma retry mantığı — token talebi geçici
    /// "kullanıcı bulunamadı" hatasıyla dönerse bekleyip tekrar denenir.
    /// </summary>
    public class TokenRetryTests
    {
        [Fact]
        public async Task RetryToken_RetriesUntilSuccess()
        {
            var calls = 0;
            var delays = 0;

            var res = await LogoRestServiceProvider.RetryTokenAsync(() =>
            {
                calls++;
                return Task.FromResult(calls < 3
                    ? ResponseDto<string>.FailData(401, "Token alınamadı", "kullanıcı bulunamadı", true)
                    : ResponseDto<string>.SuccessData(200, "ok", "TOKEN123"));
            }, maxAttempts: 3, delay: () => { delays++; return Task.CompletedTask; });

            res.IsSuccess.Should().BeTrue();
            res.Data.Should().Be("TOKEN123");
            calls.Should().Be(3);
            delays.Should().Be(2); // son denemeden sonra beklemez
        }

        [Fact]
        public async Task RetryToken_AllFail_ReturnsLastFailure()
        {
            var calls = 0;

            var res = await LogoRestServiceProvider.RetryTokenAsync(() =>
            {
                calls++;
                return Task.FromResult(
                    ResponseDto<string>.FailData(401, "Token alınamadı", "kullanıcı bulunamadı", true));
            }, maxAttempts: 3, delay: () => Task.CompletedTask);

            res.IsSuccess.Should().BeFalse();
            res.StatusCode.Should().Be(401);
            res.Message.Should().Be("Token alınamadı");
            calls.Should().Be(3);
        }
    }
}
