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
/// Unit tests for SozlesmeRepository
/// </summary>
public class SozlesmeRepositoryTests : IClassFixture<YonetimTestFixture>
{
    private readonly YonetimTestFixture _fixture;

    public SozlesmeRepositoryTests(YonetimTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveSozlesmeler()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var sozlesme1 = new Sozlesme
        {
            Firma = "Firma A",
            Konu = "Konu A",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active
        };
        var sozlesme2 = new Sozlesme
        {
            Firma = "Firma B",
            Konu = "Konu B",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Deleted
        };

        await context.Sozlesmeler.AddRangeAsync(sozlesme1, sozlesme2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Firma.Should().Be("Firma A");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnSozlesmeWithIlgiliKisiler()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var mail = TestDataHelper.CreateTestMail("Ahmet", "Yılmaz", "ahmet@test.com");
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        var sozlesme = new Sozlesme
        {
            Firma = "Test Firma",
            Konu = "Test Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        var kisi = new SozlesmeKisi { SozlesmeID = sozlesme.SozlesmeID, MailID = mail.MailID };
        await context.SozlesmeKisiler.AddAsync(kisi);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetByIdAsync(sozlesme.SozlesmeID);

        // Assert
        result.Should().NotBeNull();
        result!.Firma.Should().Be("Test Firma");
        result.IlgiliKisiler.Should().HaveCount(1);
        result.IlgiliKisiler.First().Mail!.Ad.Should().Be("Ahmet");
    }

    [Fact]
    public async Task GetExpiringContractsAsync_ShouldReturnContractsExpiringWithinDays()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var sozlesmeExpiring = new Sozlesme
        {
            Firma = "Expiring Firma",
            Konu = "Expiring Konu",
            Baslangic = DateTime.Now.AddMonths(-11),
            Bitis = DateTime.Now.AddDays(15),
            Status = StatusEnum.Active,
            Bitti = false
        };
        var sozlesmeNotExpiring = new Sozlesme
        {
            Firma = "Not Expiring Firma",
            Konu = "Not Expiring Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddMonths(2),
            Status = StatusEnum.Active,
            Bitti = false
        };
        var sozlesmeFinished = new Sozlesme
        {
            Firma = "Finished Firma",
            Konu = "Finished Konu",
            Baslangic = DateTime.Now.AddYears(-2),
            Bitis = DateTime.Now.AddDays(-5),
            Status = StatusEnum.Active,
            Bitti = false
        };

        await context.Sozlesmeler.AddRangeAsync(sozlesmeExpiring, sozlesmeNotExpiring, sozlesmeFinished);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetExpiringContractsAsync(30);

        // Assert
        result.Should().HaveCount(1);
        result.First().Firma.Should().Be("Expiring Firma");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewSozlesme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var sozlesme = new Sozlesme
        {
            Firma = "Yeni Firma",
            Konu = "Yeni Konu",
            Tur = "Hizmet",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Birim = "IT"
        };

        // Act
        var result = await sut.CreateAsync(sozlesme);

        // Assert
        result.Should().BeTrue();
        var saved = await context.Sozlesmeler.FirstOrDefaultAsync(s => s.Firma == "Yeni Firma");
        saved.Should().NotBeNull();
        saved!.Bitti.Should().BeFalse();
        saved.Arsiv.Should().BeFalse();
        saved.Status.Should().Be(StatusEnum.Active);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingSozlesme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var sozlesme = new Sozlesme
        {
            Firma = "Guncellenecek Firma",
            Konu = "Eski Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        // Act
        sozlesme.Konu = "Yeni Konu";
        sozlesme.Tur = "Lisans";
        var result = await sut.UpdateAsync(sozlesme);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Sozlesmeler.FirstOrDefaultAsync(s => s.SozlesmeID == sozlesme.SozlesmeID);
        updated!.Konu.Should().Be("Yeni Konu");
        updated!.Tur.Should().Be("Lisans");
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteSozlesme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var sozlesme = new Sozlesme
        {
            Firma = "Silinecek Firma",
            Konu = "Silinecek Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.DeleteAsync(sozlesme.SozlesmeID);

        // Assert
        result.Should().BeTrue();
        var deleted = await context.Sozlesmeler.FirstOrDefaultAsync(s => s.SozlesmeID == sozlesme.SozlesmeID);
        deleted.Should().NotBeNull();
        deleted!.Status.Should().Be(StatusEnum.Deleted);
    }

    [Fact]
    public async Task GetContractPdfAsync_ShouldReturnPdfBytes()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // PDF header
        var sozlesme = new Sozlesme
        {
            Firma = "PDF Firma",
            Konu = "PDF Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Pdf = pdfBytes,
            Status = StatusEnum.Active
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetContractPdfAsync(sozlesme.SozlesmeID);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(pdfBytes);
    }

    [Fact]
    public async Task UpdateContractStatusAsync_ShouldUpdateContractStatus()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var sozlesme = new Sozlesme
        {
            Firma = "Durum Firma",
            Konu = "Durum Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active,
            Bitti = false
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.UpdateContractStatusAsync(sozlesme.SozlesmeID, true, "Test User");

        // Assert
        result.Should().BeTrue();
        var updated = await context.Sozlesmeler.FirstOrDefaultAsync(s => s.SozlesmeID == sozlesme.SozlesmeID);
        updated!.Bitti.Should().BeTrue();
        updated.SonKisi.Should().Be("Test User");
        updated.SonTarih.Should().NotBeNull();
    }

    [Fact]
    public async Task AddIlgiliKisiAsync_ShouldAddPersonToSozlesme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var mail = TestDataHelper.CreateTestMail("Mehmet", "Demir", "mehmet@test.com");
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        var sozlesme = new Sozlesme
        {
            Firma = "Kisi Firma",
            Konu = "Kisi Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        var kisi = new SozlesmeKisi { SozlesmeID = sozlesme.SozlesmeID, MailID = mail.MailID };

        // Act
        var result = await sut.AddIlgiliKisiAsync(kisi);

        // Assert
        result.Should().BeTrue();
        var added = await context.SozlesmeKisiler.FirstOrDefaultAsync(sk => sk.SozlesmeID == sozlesme.SozlesmeID);
        added.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveIlgiliKisiAsync_ShouldRemovePersonFromSozlesme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var mail = TestDataHelper.CreateTestMail("Ayşe", "Yıldız", "ayse@test.com");
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        var sozlesme = new Sozlesme
        {
            Firma = "Kisi Sil Firma",
            Konu = "Kisi Sil Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        var kisi = new SozlesmeKisi { SozlesmeID = sozlesme.SozlesmeID, MailID = mail.MailID };
        await context.SozlesmeKisiler.AddAsync(kisi);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.RemoveIlgiliKisiAsync(kisi.SozlesmeKisiID);

        // Assert
        result.Should().BeTrue();
        var removed = await context.SozlesmeKisiler.FirstOrDefaultAsync(sk => sk.SozlesmeKisiID == kisi.SozlesmeKisiID);
        removed.Should().BeNull();
    }

    [Fact]
    public async Task GetIlgiliKisilerAsync_ShouldReturnPeopleForSozlesme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new SozlesmeRepository(context);

        // Arrange
        var mail1 = TestDataHelper.CreateTestMail("Ali", "Veli", "ali@test.com");
        var mail2 = TestDataHelper.CreateTestMail("Zeynep", "Kaya", "zeynep@test.com");
        await context.MailAdresleri.AddRangeAsync(mail1, mail2);
        await context.SaveChangesAsync();

        var sozlesme = new Sozlesme
        {
            Firma = "Coklu Kisi Firma",
            Konu = "Coklu Kisi Konu",
            Baslangic = DateTime.Now,
            Bitis = DateTime.Now.AddYears(1),
            Status = StatusEnum.Active
        };
        await context.Sozlesmeler.AddAsync(sozlesme);
        await context.SaveChangesAsync();

        var kisi1 = new SozlesmeKisi { SozlesmeID = sozlesme.SozlesmeID, MailID = mail1.MailID };
        var kisi2 = new SozlesmeKisi { SozlesmeID = sozlesme.SozlesmeID, MailID = mail2.MailID };
        await context.SozlesmeKisiler.AddRangeAsync(kisi1, kisi2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetIlgiliKisilerAsync(sozlesme.SozlesmeID);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(k => k.SozlesmeID == sozlesme.SozlesmeID);
    }
}
