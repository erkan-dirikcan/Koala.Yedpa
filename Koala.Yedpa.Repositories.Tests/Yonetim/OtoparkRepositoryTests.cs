using Koala.Yedpa.Core.Dtos;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Repositories.Repositories.Yonetim;
using Koala.Yedpa.Repositories.Tests;
using Xunit;

namespace Koala.Yedpa.Repositories.Tests.Yonetim;

/// <summary>
/// Unit tests for OtoparkRepository
/// </summary>
public class OtoparkRepositoryTests : IClassFixture<YonetimTestFixture>
{
    private readonly YonetimTestFixture _fixture;

    public OtoparkRepositoryTests(YonetimTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveOtoparkKayitlari()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit1 = new OtoparkKayit
        {
            Plaka = "34ABC123",
            GirisTarih = DateTime.Now,
            AboneAd = "Test Abone",
            Status = StatusEnum.Active
        };
        var kayit2 = new OtoparkKayit
        {
            Plaka = "34XYZ456",
            GirisTarih = DateTime.Now,
            AboneAd = "Test Abone 2",
            Status = StatusEnum.Deleted
        };

        await context.OtoparkKayitlari.AddRangeAsync(kayit1, kayit2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Plaka.Should().Be("34ABC123");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOtoparkKayit()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit = new OtoparkKayit
        {
            Plaka = "34DEF789",
            GirisTarih = DateTime.Now,
            CikisTarih = null,
            AboneAd = "Test Abone",
            Telefon = "5551234567",
            Status = StatusEnum.Active
        };
        await context.OtoparkKayitlari.AddAsync(kayit);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetByIdAsync(kayit.KayitID);

        // Assert
        result.Should().NotBeNull();
        result!.Plaka.Should().Be("34DEF789");
        result.AboneAd.Should().Be("Test Abone");
    }

    [Fact]
    public async Task GetByPlakaAsync_ShouldReturnLatestKayitForPlaka()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit1 = new OtoparkKayit
        {
            Plaka = "34AAA111",
            GirisTarih = DateTime.Now.AddDays(-1),
            CikisTarih = DateTime.Now.AddDays(-1).AddHours(2),
            Status = StatusEnum.Active
        };
        var kayit2 = new OtoparkKayit
        {
            Plaka = "34AAA111",
            GirisTarih = DateTime.Now,
            CikisTarih = null,
            Status = StatusEnum.Active
        };

        await context.OtoparkKayitlari.AddRangeAsync(kayit1, kayit2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetByPlakaAsync("34AAA111");

        // Assert
        result.Should().NotBeNull();
        result!.KayitID.Should().Be(kayit2.KayitID); // Latest entry
    }

    [Fact]
    public async Task GetActiveSubscriptionsAsync_ShouldReturnKayitsWithoutCikisTarih()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var aktifKayit = new OtoparkKayit
        {
            Plaka = "34ACT123",
            GirisTarih = DateTime.Now,
            CikisTarih = null,
            AboneAd = "Aktif Abone",
            Status = StatusEnum.Active
        };
        var pasifKayit = new OtoparkKayit
        {
            Plaka = "34PAS456",
            GirisTarih = DateTime.Now.AddHours(-5),
            CikisTarih = DateTime.Now.AddHours(-3),
            AboneAd = "Pasif Abone",
            Status = StatusEnum.Active
        };

        await context.OtoparkKayitlari.AddRangeAsync(aktifKayit, pasifKayit);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetActiveSubscriptionsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Plaka.Should().Be("34ACT123");
    }

    [Fact]
    public async Task GirisYapAsync_ShouldCreateNewKayit()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit = new OtoparkKayit
        {
            Plaka = "34NEW999",
            AboneAd = "Yeni Abone",
            Telefon = "5559876543"
        };

        // Act
        var result = await sut.GirisYapAsync(kayit);

        // Assert
        result.Should().BeTrue();
        var saved = await context.OtoparkKayitlari.FirstOrDefaultAsync(k => k.Plaka == "34NEW999");
        saved.Should().NotBeNull();
        saved!.GirisTarih.Should().NotBeNull();
        saved!.CikisTarih.Should().BeNull();
        saved!.Status.Should().Be(StatusEnum.Active);
    }

    [Fact]
    public async Task CikisYapAsync_ShouldUpdateCikisTarih()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit = new OtoparkKayit
        {
            Plaka = "34OUT888",
            GirisTarih = DateTime.Now.AddHours(-2),
            CikisTarih = null,
            Status = StatusEnum.Active
        };
        await context.OtoparkKayitlari.AddAsync(kayit);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.CikisYapAsync("34OUT888");

        // Assert
        result.Should().BeTrue();
        var updated = await context.OtoparkKayitlari.FirstOrDefaultAsync(k => k.Plaka == "34OUT888");
        updated.Should().NotBeNull();
        updated!.CikisTarih.Should().NotBeNull();
    }

    [Fact]
    public async Task CikisYapAsync_ShouldReturnFalse_WhenNoAktifKayitExists()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange - No active kayit for this plate
        var kayit = new OtoparkKayit
        {
            Plaka = "34NOACT",
            GirisTarih = DateTime.Now.AddHours(-5),
            CikisTarih = DateTime.Now.AddHours(-3),
            Status = StatusEnum.Active
        };
        await context.OtoparkKayitlari.AddAsync(kayit);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.CikisYapAsync("34NOACT");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AboneEkleAsync_ShouldCreateNewAbonelik()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit = new OtoparkKayit
        {
            Plaka = "34SUB777",
            AboneAd = "Abone User",
            Telefon = "5555555555"
        };

        // Act
        var result = await sut.AboneEkleAsync(kayit);

        // Assert
        result.Should().BeTrue();
        var saved = await context.OtoparkKayitlari.FirstOrDefaultAsync(k => k.Plaka == "34SUB777");
        saved.Should().NotBeNull();
        saved!.GirisTarih.Should().NotBeNull();
        saved!.CikisTarih.Should().BeNull(); // Abonelik için null
        saved!.Status.Should().Be(StatusEnum.Active);
    }

    [Fact]
    public async Task AboneGuncelleAsync_ShouldUpdateAbone()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit = new OtoparkKayit
        {
            Plaka = "34UPD666",
            GirisTarih = DateTime.Now,
            CikisTarih = null,
            AboneAd = "Eski Abone",
            Telefon = "5551111111",
            Status = StatusEnum.Active
        };
        await context.OtoparkKayitlari.AddAsync(kayit);
        await context.SaveChangesAsync();

        // Act
        kayit.AboneAd = "Yeni Abone";
        kayit.Telefon = "5552222222";
        var result = await sut.AboneGuncelleAsync(kayit);

        // Assert
        result.Should().BeTrue();
        var updated = await context.OtoparkKayitlari.FirstOrDefaultAsync(k => k.KayitID == kayit.KayitID);
        updated.Should().NotBeNull();
        updated!.AboneAd.Should().Be("Yeni Abone");
        updated!.Telefon.Should().Be("5552222222");
    }

    [Fact]
    public async Task AboneGuncelleAsync_ShouldReturnFalse_WhenAboneNotFound()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange - kayit with KayitID that doesn't exist
        var kayit = new OtoparkKayit
        {
            KayitID = 999,
            Plaka = "34NOTFND",
            AboneAd = "Bulunamayan Abone"
        };

        // Act
        var result = await sut.AboneGuncelleAsync(kayit);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task AboneSilAsync_ShouldSoftDeleteAbone()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Arrange
        var kayit = new OtoparkKayit
        {
            Plaka = "34DEL555",
            GirisTarih = DateTime.Now,
            CikisTarih = null,
            AboneAd = "Silinecek Abone",
            Status = StatusEnum.Active
        };
        await context.OtoparkKayitlari.AddAsync(kayit);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.AboneSilAsync(kayit.KayitID);

        // Assert
        result.Should().BeTrue();
        var deleted = await context.OtoparkKayitlari.FirstOrDefaultAsync(k => k.KayitID == kayit.KayitID);
        deleted.Should().NotBeNull();
        deleted!.Status.Should().Be(StatusEnum.Deleted);
    }

    [Fact]
    public async Task AboneSilAsync_ShouldReturnFalse_WhenAboneNotFound()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OtoparkRepository(context);

        // Act - Try to delete non-existent kayit
        var result = await sut.AboneSilAsync(999);

        // Assert
        result.Should().BeFalse();
    }
}
