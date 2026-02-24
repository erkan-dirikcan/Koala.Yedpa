using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Koala.Yedpa.Repositories;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.Yonetim;

namespace Koala.Yedpa.Repositories.Tests;

/// <summary>
/// Base test fixture for creating in-memory DbContext for repository tests
/// </summary>
public class YonetimTestFixture : IDisposable
{
    private readonly DbContextOptions<AppDbContext> _options;

    public YonetimTestFixture()
    {
        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;
    }

    public AppDbContext CreateContext()
    {
        var context = new AppDbContext(_options);
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();
        return context;
    }

    public void Dispose()
    {
        // Context disposal is handled by individual tests
    }
}

/// <summary>
/// Helper methods for seeding test data
/// </summary>
public static class TestDataHelper
{
    public static Mail CreateTestMail(string ad = "Test", string soyad = "User", string ePosta = "test@example.com")
    {
        return new Mail
        {
            Ad = ad,
            Soyad = soyad,
            EPosta = ePosta,
            GSM = "5551234567",
            Telefon = "5559876543"
        };
    }

    public static Birim CreateTestBirim(string birimAdi = "Test Birimi")
    {
        return new Birim
        {
            BirimAdi = birimAdi
        };
    }

    public static Durum CreateTestDurum(string durumAdi = "Beklemede")
    {
        return new Durum
        {
            DurumAdi = durumAdi
        };
    }
}
