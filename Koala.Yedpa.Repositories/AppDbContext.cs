using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Models.Yonetim;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Module = Koala.Yedpa.Core.Models.Module;

namespace Koala.Yedpa.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, AppRole, string>(options)
{
    public DbSet<BudgetRatio> BudgetRatio { get; set; }
    public DbSet<Claims> Claims { get; set; }
    public DbSet<DuesStatistic> DuesStatistics { get; set; }
    public DbSet<QRCodeBatch> QRCodeBatches { get; set; }
    public DbSet<QRCode> QRCodes { get; set; }
    public DbSet<Workplace> Workplace { get; set; }
    public DbSet<EmailTemplate> EmailTemplate { get; set; }
    public DbSet<ExtendedProperties> ExtendedProperties { get; set; }
    public DbSet<ExtendedPropertyRecordValues> ExtendedPropertyRecordValues { get; set; }
    public DbSet<ExtendedPropertyValues> ExtendedPropertyValues { get; set; }
    public DbSet<GeneratedIds> GeneratedIds { get; set; }
    public DbSet<Module> Module { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<Transaction> Transaction { get; set; }
    public DbSet<TransactionItem> TransactionItem { get; set; }
    public DbSet<TransactionType> TransactionType { get; set; }

    // YONETIM Database Entities
    public DbSet<Raf> Raflar { get; set; }
    public DbSet<Bolme> Bolumeler { get; set; }
    public DbSet<Koli> Koliler { get; set; }
    public DbSet<Sozlesme> Sozlesmeler { get; set; }
    public DbSet<SozlesmeKisi> SozlesmeKisiler { get; set; }
    public DbSet<Ariza> Arizalar { get; set; }
    public DbSet<ArizaHareket> ArizaHareketleri { get; set; }
    public DbSet<ArizaKisi> ArizaKisiler { get; set; }
    public DbSet<OtoparkKayit> OtoparkKayitlari { get; set; }
    public DbSet<Mail> MailAdresleri { get; set; }
    public DbSet<Birim> Birimler { get; set; }
    public DbSet<Durum> Durumlar { get; set; }
    //public DbSet<>  { get; set; }
    //public DbSet<>  { get; set; }
    //public DbSet<>  { get; set; }
    //public DbSet<>  { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);

        builder.Entity<Settings>()
            .Property(s => s.CreateTime)
            .HasDefaultValueSql("GETUTCDATE()");
    }
}