using Koala.Yedpa.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using Module = Koala.Yedpa.Core.Models.Module;

namespace Koala.Yedpa.Repositories;

public class AppDbContext(DbContextOptions<AppDbContext> options) : IdentityDbContext<AppUser, AppRole, string>(options)
{
    public DbSet<Claims> Claims { get; set; }
    public DbSet<EmailTemplate> EmailTemplate { get; set; }
    public DbSet<ExtendedProperties> ExtendedProperties { get; set; }
    public DbSet<ExtendedPropertyRecordValues> ExtendedPropertyRecordValues { get; set; }
    public DbSet<ExtendedPropertyValues> ExtendedPropertyValues { get; set; }
    public DbSet<GeneratedIds> GeneratedIds { get; set; }
    public DbSet<LgXt001211> LgXt001211 { get; set; }
    public DbSet<Module> Module { get; set; }
    public DbSet<Settings> Settings { get; set; }
    public DbSet<Transaction> Transaction { get; set; }
    public DbSet<TransactionItem> TransactionItem { get; set; }
    public DbSet<TransactionType> TransactionType { get; set; }
    //public DbSet<>  { get; set; }
    //public DbSet<>  { get; set; }
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