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
/// Unit tests for ArsivRepository
/// </summary>
public class ArsivRepositoryTests : IClassFixture<YonetimTestFixture>
{
    private readonly YonetimTestFixture _fixture;

    public ArsivRepositoryTests(YonetimTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllRaflarAsync_ShouldReturnAllRaflarWithBolumelerAndKoliler()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF01", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        await context.Bolumeler.AddAsync(bolme);
        await context.SaveChangesAsync();

        var koli = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K1", Detay = "Test Koli", Status = StatusEnum.Active };
        await context.Koliler.AddAsync(koli);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetAllRaflarAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().ContainSingle(r => r.RafKod == "RAF01");
        result.First().Bolumeler.Should().NotBeEmpty();
        result.First().Bolumeler.First().Koliler.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetRafByIdAsync_ShouldReturnRafWithBolumelerAndKoliler()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF02", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetRafByIdAsync(raf.RafID);

        // Assert
        result.Should().NotBeNull();
        result!.RafKod.Should().Be("RAF02");
        result.Bolumeler.Should().NotBeNull();
    }

    [Fact]
    public async Task AddRafAsync_ShouldAddNewRaf()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_NEW" };

        // Act
        var result = await sut.AddRafAsync(raf);

        // Assert
        result.Should().BeTrue();
        var savedRaf = await context.Raflar.FirstOrDefaultAsync(r => r.RafKod == "RAF_NEW");
        savedRaf.Should().NotBeNull();
        savedRaf!.Status.Should().Be(StatusEnum.Active);
    }

    [Fact]
    public async Task UpdateRafAsync_ShouldUpdateExistingRaf()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_UPDATE", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        // Act
        raf.RafKod = "RAF_UPDATED";
        var result = await sut.UpdateRafAsync(raf);

        // Assert
        result.Should().BeTrue();
        var updatedRaf = await context.Raflar.FirstOrDefaultAsync(r => r.RafID == raf.RafID);
        updatedRaf!.RafKod.Should().Be("RAF_UPDATED");
    }

    [Fact]
    public async Task DeleteRafAsync_ShouldSoftDeleteRaf()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_DELETE", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.DeleteRafAsync(raf.RafID);

        // Assert
        result.Should().BeTrue();
        var deletedRaf = await context.Raflar.FirstOrDefaultAsync(r => r.RafID == raf.RafID);
        deletedRaf.Should().NotBeNull();
        deletedRaf!.Status.Should().Be(StatusEnum.Deleted);
    }

    [Fact]
    public async Task GetBolumelerByRafAsync_ShouldReturnBolumelerForRaf()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_BOLME", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme1 = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        var bolme2 = new Bolme { RafID = raf.RafID, BolmeNo = "B2", Status = StatusEnum.Active };
        await context.Bolumeler.AddRangeAsync(bolme1, bolme2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetBolumelerByRafAsync(raf.RafID);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(b => b.RafID == raf.RafID);
    }

    [Fact]
    public async Task AddBolmeAsync_ShouldAddNewBolme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_BOLME_ADD", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B_NEW" };

        // Act
        var result = await sut.AddBolmeAsync(bolme);

        // Assert
        result.Should().BeTrue();
        var savedBolme = await context.Bolumeler.FirstOrDefaultAsync(b => b.BolmeNo == "B_NEW");
        savedBolme.Should().NotBeNull();
    }

    [Fact]
    public async Task GetKolilerByBolmeAsync_ShouldReturnKolilerForBolme()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_KOLI", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        await context.Bolumeler.AddAsync(bolme);
        await context.SaveChangesAsync();

        var koli1 = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K1", Status = StatusEnum.Active };
        var koli2 = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K2", Status = StatusEnum.Active };
        await context.Koliler.AddRangeAsync(koli1, koli2);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetKolilerByBolmeAsync(bolme.BolmeID);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(k => k.BolmeID == bolme.BolmeID);
    }

    [Fact]
    public async Task AddKoliAsync_ShouldAddNewKoli()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_KOLI_ADD", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        await context.Bolumeler.AddAsync(bolme);
        await context.SaveChangesAsync();

        var koli = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K_NEW", Detay = "Test Detay" };

        // Act
        var result = await sut.AddKoliAsync(koli);

        // Assert
        result.Should().BeTrue();
        var savedKoli = await context.Koliler.FirstOrDefaultAsync(k => k.KoliNo == "K_NEW");
        savedKoli.Should().NotBeNull();
        savedKoli!.Detay.Should().Be("Test Detay");
    }

    [Fact]
    public async Task UpdateKoliAsync_ShouldUpdateExistingKoli()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_KOLI_UPD", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        await context.Bolumeler.AddAsync(bolme);
        await context.SaveChangesAsync();

        var koli = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K_UPD", Detay = "Eski Detay", Status = StatusEnum.Active };
        await context.Koliler.AddAsync(koli);
        await context.SaveChangesAsync();

        // Act
        koli.Detay = "Yeni Detay";
        var result = await sut.UpdateKoliAsync(koli);

        // Assert
        result.Should().BeTrue();
        var updatedKoli = await context.Koliler.FirstOrDefaultAsync(k => k.KoliID == koli.KoliID);
        updatedKoli!.Detay.Should().Be("Yeni Detay");
    }

    [Fact]
    public async Task DeleteKoliAsync_ShouldSoftDeleteKoli()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_KOLI_DEL", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        await context.Bolumeler.AddAsync(bolme);
        await context.SaveChangesAsync();

        var koli = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K_DEL", Status = StatusEnum.Active };
        await context.Koliler.AddAsync(koli);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.DeleteKoliAsync(koli.KoliID);

        // Assert
        result.Should().BeTrue();
        var deletedKoli = await context.Koliler.FirstOrDefaultAsync(k => k.KoliID == koli.KoliID);
        deletedKoli.Should().NotBeNull();
        deletedKoli!.Status.Should().Be(StatusEnum.Deleted);
    }

    [Fact]
    public async Task GetArsivListesiAsync_ShouldReturnHierarchicalStructure()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_LIST", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        await context.Bolumeler.AddAsync(bolme);
        await context.SaveChangesAsync();

        var koli = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K1", Detay = "List Test", Status = StatusEnum.Active };
        await context.Koliler.AddAsync(koli);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetArsivListesiAsync();

        // Assert
        result.Should().NotBeEmpty();
        result.Should().ContainSingle();
        result.First().RafKod.Should().Be("RAF_LIST");
        result.First().BolmeNo.Should().Be("B1");
        result.First().KoliNo.Should().Be("K1");
    }

    [Fact]
    public async Task GetKoliDetayAsync_ShouldReturnKoliDetails()
    {
        await using var context = _fixture.CreateContext();
        var sut = new ArsivRepository(context);

        // Arrange
        var raf = new Raf { RafKod = "RAF_DETAY", Status = StatusEnum.Active };
        await context.Raflar.AddAsync(raf);
        await context.SaveChangesAsync();

        var bolme = new Bolme { RafID = raf.RafID, BolmeNo = "B1", Status = StatusEnum.Active };
        await context.Bolumeler.AddAsync(bolme);
        await context.SaveChangesAsync();

        var koli = new Koli { BolmeID = bolme.BolmeID, KoliNo = "K_DETAY", Detay = "Detay Test", Status = StatusEnum.Active };
        await context.Koliler.AddAsync(koli);
        await context.SaveChangesAsync();

        // Act
        var result = await sut.GetKoliDetayAsync(koli.KoliID);

        // Assert
        result.Should().NotBeNull();
        result!.RafKod.Should().Be("RAF_DETAY");
        result!.BolmeNo.Should().Be("B1");
        result!.KoliNo.Should().Be("K_DETAY");
        result!.Detay.Should().Be("Detay Test");
    }
}
