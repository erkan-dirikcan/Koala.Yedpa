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
/// Unit tests for ArizaRepository
/// </summary>
public class ArizaRepositoryTests : IClassFixture<YonetimTestFixture>
{
    private readonly YonetimTestFixture _fixture;

    public ArizaRepositoryTests(YonetimTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllActiveArizalar()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza1 = new Ariza
        {
            FirmaAdres = "Firma A",
            Konu = "Arıza A",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        var ariza2 = new Ariza
        {
            FirmaAdres = "Firma B",
            Konu = "Arıza B",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Deleted
        };

        await context.Arizalar.AddRangeAsync(ariza1, ariza2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Konu.Should().Be("Arıza A");
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnArizaWithHareketlerAndIlgiliKisiler()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var mail = TestDataHelper.CreateTestMail("Test", "User", "test@test.com");
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        var ariza = new Ariza
        {
            FirmaAdres = "Test Firma",
            Konu = "Test Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        var hareket = new ArizaHareket
        {
            ArizaID = ariza.ArizaID,
            Aciklama = "İlk hareket",
            Kisi = "Admin",
            Tarih = DateTime.Now
        };
        await context.ArizaHareketleri.AddAsync(hareket);

        var kisi = new ArizaKisi { ArizaID = ariza.ArizaID, MailID = mail.MailID };
        await context.ArizaKisiler.AddAsync(kisi);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetByIdAsync(ariza.ArizaID);

        // Assert
        result.Should().NotBeNull();
        result!.Konu.Should().Be("Test Arıza");
        result.Hareketler.Should().HaveCount(1);
        result.IlgiliKisiler.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByBirimAsync_ShouldReturnArizalarForBirim()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza1 = new Ariza
        {
            FirmaAdres = "Firma A",
            Konu = "IT Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        var ariza2 = new Ariza
        {
            FirmaAdres = "Firma B",
            Konu = "HR Arıza",
            Tarih = DateTime.Now,
            Birim = "HR",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };

        await context.Arizalar.AddRangeAsync(ariza1, ariza2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetByBirimAsync("IT");

        // Assert
        result.Should().HaveCount(1);
        result.First().Birim.Should().Be("IT");
    }

    [Fact]
    public async Task GetActiveFaultsAsync_ShouldReturnUnfinishedArizalar()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var arizaActive = new Ariza
        {
            FirmaAdres = "Aktif Firma",
            Konu = "Aktif Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Devam Ediyor",
            Status = StatusEnum.Active
        };
        var arizaFinished = new Ariza
        {
            FirmaAdres = "Biten Firma",
            Konu = "Biten Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Tamamlandı",
            Status = StatusEnum.Active
        };

        await context.Arizalar.AddRangeAsync(arizaActive, arizaFinished);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetActiveFaultsAsync();

        // Assert
        result.Should().HaveCount(1);
        result.First().Konu.Should().Be("Aktif Arıza");
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateNewAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza = new Ariza
        {
            FirmaAdres = "Yeni Firma",
            Konu = "Yeni Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Gizli = false
        };

        // Act
        var result = await sut.CreateAsync(ariza);

        // Assert
        result.Should().BeTrue();
        var saved = await context.Arizalar.FirstOrDefaultAsync(a => a.Konu == "Yeni Arıza");
        saved.Should().NotBeNull();
        saved!.Status.Should().Be(StatusEnum.Active);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateExistingAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza = new Ariza
        {
            FirmaAdres = "Guncellenecek Firma",
            Konu = "Eski Konu",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        // Act
        ariza.Konu = "Yeni Konu";
        ariza.Gizli = true;
        var result = await sut.UpdateAsync(ariza);

        // Assert
        result.Should().BeTrue();
        var updated = await context.Arizalar.FirstOrDefaultAsync(a => a.ArizaID == ariza.ArizaID);
        updated!.Konu.Should().Be("Yeni Konu");
        updated!.Gizli.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_ShouldSoftDeleteAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza = new Ariza
        {
            FirmaAdres = "Silinecek Firma",
            Konu = "Silinecek Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.DeleteAsync(ariza.ArizaID);

        // Assert
        result.Should().BeTrue();
        var deleted = await context.Arizalar.FirstOrDefaultAsync(a => a.ArizaID == ariza.ArizaID);
        deleted.Should().NotBeNull();
        deleted!.Status.Should().Be(StatusEnum.Deleted);
    }

    [Fact]
    public async Task UpdateDurumAsync_ShouldUpdateArizaDurum()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza = new Ariza
        {
            FirmaAdres = "Durum Firma",
            Konu = "Durum Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.UpdateDurumAsync(ariza.ArizaID, "Tamamlandı", "Test User");

        // Assert
        result.Should().BeTrue();
        var updated = await context.Arizalar.FirstOrDefaultAsync(a => a.ArizaID == ariza.ArizaID);
        updated!.Durum.Should().Be("Tamamlandı");
        updated!.SonKisi.Should().Be("Test User");
        updated!.SonTarih.Should().NotBeNull();
    }

    [Fact]
    public async Task AddHareketAsync_ShouldAddHareketToAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza = new Ariza
        {
            FirmaAdres = "Hareket Firma",
            Konu = "Hareket Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        var hareket = new ArizaHareket
        {
            ArizaID = ariza.ArizaID,
            Aciklama = "Test hareketi",
            Kisi = "Admin"
        };

        // Act
        var result = await sut.AddHareketAsync(hareket);

        // Assert
        result.Should().BeTrue();
        var added = await context.ArizaHareketleri.FirstOrDefaultAsync(h => h.ArizaID == ariza.ArizaID);
        added.Should().NotBeNull();
        added!.Aciklama.Should().Be("Test hareketi");
    }

    [Fact]
    public async Task GetHareketlerAsync_ShouldReturnHareketlerForAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var ariza = new Ariza
        {
            FirmaAdres = "Coklu Hareket Firma",
            Konu = "Coklu Hareket Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        var hareket1 = new ArizaHareket { ArizaID = ariza.ArizaID, Aciklama = "Hareket 1", Kisi = "Admin", Tarih = DateTime.Now.AddMinutes(-10) };
        var hareket2 = new ArizaHareket { ArizaID = ariza.ArizaID, Aciklama = "Hareket 2", Kisi = "User", Tarih = DateTime.Now };
        await context.ArizaHareketleri.AddRangeAsync(hareket1, hareket2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetHareketlerAsync(ariza.ArizaID);

        // Assert
        result.Should().HaveCount(2);
        result.Should().BeInDescendingOrder(h => h.Tarih);
    }

    [Fact]
    public async Task AddIlgiliKisiAsync_ShouldAddPersonToAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var mail = TestDataHelper.CreateTestMail("Ariza", "User", "ariza@test.com");
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        var ariza = new Ariza
        {
            FirmaAdres = "Kisi Firma",
            Konu = "Kisi Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        var kisi = new ArizaKisi { ArizaID = ariza.ArizaID, MailID = mail.MailID };

        // Act
        var result = await sut.AddIlgiliKisiAsync(kisi);

        // Assert
        result.Should().BeTrue();
        var added = await context.ArizaKisiler.FirstOrDefaultAsync(ak => ak.ArizaID == ariza.ArizaID);
        added.Should().NotBeNull();
    }

    [Fact]
    public async Task RemoveIlgiliKisiAsync_ShouldRemovePersonFromAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var mail = TestDataHelper.CreateTestMail("Sil", "User", "sil@test.com");
        await context.MailAdresleri.AddAsync(mail);
        await context.SaveChangesAsync();

        var ariza = new Ariza
        {
            FirmaAdres = "Kisi Sil Firma",
            Konu = "Kisi Sil Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        var kisi = new ArizaKisi { ArizaID = ariza.ArizaID, MailID = mail.MailID };
        await context.ArizaKisiler.AddAsync(kisi);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.RemoveIlgiliKisiAsync(kisi.ArizaKisiID);

        // Assert
        result.Should().BeTrue();
        var removed = await context.ArizaKisiler.FirstOrDefaultAsync(ak => ak.ArizaKisiID == kisi.ArizaKisiID);
        removed.Should().BeNull();
    }

    [Fact]
    public async Task GetIlgiliKisilerAsync_ShouldReturnPeopleForAriza()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArizaRepository(context);

        // Arrange
        var mail1 = TestDataHelper.CreateTestMail("Ilgi", "Kisi1", "ilgi1@test.com");
        var mail2 = TestDataHelper.CreateTestMail("Ilgi", "Kisi2", "ilgi2@test.com");
        await context.MailAdresleri.AddRangeAsync(mail1, mail2);
        await context.SaveChangesAsync();

        var ariza = new Ariza
        {
            FirmaAdres = "Coklu Kisi Firma",
            Konu = "Coklu Kisi Arıza",
            Tarih = DateTime.Now,
            Birim = "IT",
            Durum = "Beklemede",
            Status = StatusEnum.Active
        };
        await context.Arizalar.AddAsync(ariza);
        await context.SaveChangesAsync();

        var kisi1 = new ArizaKisi { ArizaID = ariza.ArizaID, MailID = mail1.MailID };
        var kisi2 = new ArizaKisi { ArizaID = ariza.ArizaID, MailID = mail2.MailID };
        await context.ArizaKisiler.AddRangeAsync(kisi1, kisi2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetIlgiliKisilerAsync(ariza.ArizaID);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(k => k.ArizaID == ariza.ArizaID);
    }
}
