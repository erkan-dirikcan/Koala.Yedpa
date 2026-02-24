using Koala.Yedpa.Core.Dtos;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Koala.Yedpa.Repositories.Repositories.Yonetim;
using Koala.Yedpa.Repositories.Tests;
using Xunit;

namespace Koala.Yedpa.Repositories.Tests.Yonetim;

/// <summary>
/// Unit tests for OrtakRepository
/// </summary>
public class OrtakRepositoryTests : IClassFixture<YonetimTestFixture>
{
    private readonly YonetimTestFixture _fixture;

    public OrtakRepositoryTests(YonetimTestFixture fixture)
    {
        _fixture = fixture;
    }

    #region Birim Tests

    [Fact]
    public async Task GetAllBirimlerAsync_ShouldReturnAllBirimlerOrdered()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var birim1 = new Birim { BirimAdi = "Z Departmanı" };
        var birim2 = new Birim { BirimAdi = "A Departmanı" };
        var birim3 = new Birim { BirimAdi = "M Departmanı" };

        await context.Birimler.AddRangeAsync(birim1, birim2, birim3);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllBirimlerAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].BirimAdi.Should().Be("A Departmanı");
        result[1].BirimAdi.Should().Be("M Departmanı");
        result[2].BirimAdi.Should().Be("Z Departmanı");
    }

    #endregion

    #region Mail Tests

    [Fact]
    public async Task GetAllMailAdresleriAsync_ShouldReturnAllMailOrderedByName()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var mail1 = new Mail { Ad = "Zeynep", Soyad = "Yılmaz", EPosta = "zeynep@test.com" };
        var mail2 = new Mail { Ad = "Ahmet", Soyad = "Demir", EPosta = "ahmet@test.com" };
        var mail3 = new Mail { Ad = "Mehmet", Soyad = "Kaya", EPosta = "mehmet@test.com" };

        await context.MailAdresleri.AddRangeAsync(mail1, mail2, mail3);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllMailAdresleriAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].Ad.Should().Be("Ahmet");
        result[1].Ad.Should().Be("Mehmet");
        result[2].Ad.Should().Be("Zeynep");
    }

    [Fact]
    public async Task GetAllMailAdresleriAsync_ShouldSortBySoyad_WhenAdSame()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var mail1 = new Mail { Ad = "Ali", Soyad = "Zeki", EPosta = "ali1@test.com" };
        var mail2 = new Mail { Ad = "Ali", Soyad = "Demir", EPosta = "ali2@test.com" };

        await context.MailAdresleri.AddRangeAsync(mail1, mail2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllMailAdresleriAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Soyad.Should().Be("Demir");
        result[1].Soyad.Should().Be("Zeki");
    }

    [Fact]
    public async Task GetMailByIdAsync_ShouldReturnMail()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var mail = new Mail { Ad = "Test", Soyad = "User", EPosta = "test@test.com" };
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetMailByIdAsync(mail.MailID);

        // Assert
        result.Should().NotBeNull();
        result!.Ad.Should().Be("Test");
        result.EPosta.Should().Be("test@test.com");
    }

    [Fact]
    public async Task GetMailByIdAsync_ShouldReturnNull_WhenNotFound()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Act
        var result = await sut.GetMailByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddMailAsync_ShouldAddNewMail()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var mail = new Mail
        {
            Ad = "Yeni",
            Soyad = "Kullanıcı",
            EPosta = "yeni@test.com",
            GSM = "5551234567",
            Telefon = "5559876543"
        };

        // Act
        var result = await sut.AddMailAsync(mail);

        // Assert
        result.Should().BeTrue();
        var saved = await context.MailAdresleri.FirstOrDefaultAsync(m => m.EPosta == "yeni@test.com");
        saved.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateMailAsync_ShouldUpdateExistingMail()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var mail = new Mail
        {
            Ad = "Guncellenecek",
            Soyad = "User",
            EPosta = "guncel@test.com"
        };
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        // Act
        mail.Ad = "Guncellenen";
        mail.GSM = "5321111111";
        var result = await sut.UpdateMailAsync(mail);

        // Assert
        result.Should().BeTrue();
        var updated = await context.MailAdresleri.FirstOrDefaultAsync(m => m.MailID == mail.MailID);
        updated.Should().NotBeNull();
        updated!.Ad.Should().Be("Guncellenen");
        updated!.GSM.Should().Be("5321111111");
    }

    [Fact]
    public async Task UpdateMailAsync_ShouldReturnFalse_WhenMailNotFound()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange - mail with MailID that doesn't exist
        var mail = new Mail
        {
            MailID = 999,
            Ad = "Bulunamayan",
            Soyad = "User",
            EPosta = "notfound@test.com"
        };

        // Act
        var result = await sut.UpdateMailAsync(mail);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteMailAsync_ShouldDeleteMail()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var mail = new Mail
        {
            Ad = "Silinecek",
            Soyad = "User",
            EPosta = "sil@test.com"
        };
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.DeleteMailAsync(mail.MailID);

        // Assert
        result.Should().BeTrue();
        var deleted = await context.MailAdresleri.FirstOrDefaultAsync(m => m.MailID == mail.MailID);
        deleted.Should().BeNull(); // Hard delete from Mail table
    }

    [Fact]
    public async Task DeleteMailAsync_ShouldReturnFalse_WhenMailNotFound()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Act
        var result = await sut.DeleteMailAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Durum Tests

    [Fact]
    public async Task GetAllDurumlarAsync_ShouldReturnAllDurumlarOrdered()
    {
        await using var context = _fixture.CreateContext();
        var sut = new OrtakRepository(context);

        // Arrange
        var durum1 = new Durum { DurumAdi = "Zeyil Durum" };
        var durum2 = new Durum { DurumAdi = "Akış Durum" };
        var durum3 = new Durum { DurumAdi = "Bitiş Durum" };

        await context.Durumlar.AddRangeAsync(durum1, durum2, durum3);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllDurumlarAsync();

        // Assert
        result.Should().HaveCount(3);
        result[0].DurumAdi.Should().Be("Akış Durum");
        result[1].DurumAdi.Should().Be("Bitiş Durum");
        result[2].DurumAdi.Should().Be("Zeyil Durum");
    }

    #endregion
}
