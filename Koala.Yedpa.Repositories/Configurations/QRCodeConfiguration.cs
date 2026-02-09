using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Repositories.Configurations;

public class QRCodeConfiguration : IEntityTypeConfiguration<QRCode>
{
    public void Configure(EntityTypeBuilder<QRCode> builder)
    {
        builder.ToTable("QRCodes");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedOnAdd()
            .UseIdentityColumn();

        builder.Property(x => x.PartnerNo)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.QRCodeNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.QRImagePath)
            .HasMaxLength(500);

        builder.Property(x => x.FolderPath)
            .HasMaxLength(500);

        builder.Property(x => x.QrCodeYear)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.CreateTime)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.Property(x => x.Status)
            .HasDefaultValue(StatusEnum.Active);

        // Index'ler
        builder.HasIndex(x => x.BatchId)
            .HasDatabaseName("IX_QRCodes_BatchId");

        builder.HasIndex(x => x.PartnerNo)
            .HasDatabaseName("IX_QRCodes_PartnerNo");

        builder.HasIndex(x => x.QRCodeNumber)
            .HasDatabaseName("IX_QRCodes_QRCodeNumber");

        builder.HasIndex(x => x.QrCodeYear)
            .HasDatabaseName("IX_QRCodes_QrCodeYear");

        builder.HasIndex(x => x.Status)
            .HasDatabaseName("IX_QRCodes_Status");

        // Foreign Key
        builder.HasOne(x => x.Batch)
            .WithMany()
            .HasForeignKey(x => x.BatchId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique Index: Aynı PartnerNo aynı yılda sadece bir kez olabilir
        builder.HasIndex(x => new { x.PartnerNo, x.QrCodeYear })
            .IsUnique()
            .HasDatabaseName("UQ_QRCodes_PartnerNo_Year");
    }
}
