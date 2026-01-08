using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Repositories.Configurations;

public class WorkplaceConfiguration : IEntityTypeConfiguration<Workplace>
{
    public void Configure(EntityTypeBuilder<Workplace> builder)
    {
        builder.ToTable("Workplace");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(36)
            .HasDefaultValueSql("(NEWID())");

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Definition)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.LogicalRef)
            .IsRequired();

        builder.Property(x => x.LogRef)
            .IsRequired();

        builder.Property(x => x.IndDivNo)
            .HasMaxLength(50);

        builder.Property(x => x.ResidenceNo)
            .HasMaxLength(50);

        builder.Property(x => x.WaterMeterNo)
            .HasMaxLength(50);

        builder.Property(x => x.CalMeterNo)
            .HasMaxLength(50);

        builder.Property(x => x.HotWaterMeterNo)
            .HasMaxLength(50);

        builder.Property(x => x.IdentityNr)
            .HasMaxLength(50);

        builder.Property(x => x.ProfitingOwner)
            .HasMaxLength(200);

        // Index'ler
        builder.HasIndex(x => x.Code)
            .HasDatabaseName("IX_Workplace_Code");

        builder.HasIndex(x => x.LogicalRef)
            .IsUnique()
            .HasDatabaseName("IX_Workplace_LogicalRef");

        builder.HasIndex(x => x.LogRef)
            .IsUnique()
            .HasDatabaseName("IX_Workplace_LogRef");
    }
}
