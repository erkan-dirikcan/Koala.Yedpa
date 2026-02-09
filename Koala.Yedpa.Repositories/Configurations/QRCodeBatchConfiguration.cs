using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Repositories.Configurations
{
    public class QRCodeBatchConfiguration : IEntityTypeConfiguration<QRCodeBatch>
    {
        public void Configure(EntityTypeBuilder<QRCodeBatch> builder)
        {
            builder.ToTable("QRCodeBatches");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd()
                .UseIdentityColumn();

            builder.Property(x => x.SqlQuery)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(x => x.QrCodeYear)
                .IsRequired()
                .HasMaxLength(10);

            builder.Property(x => x.QrCodePreCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.CreateTime)
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.Status)
                .HasDefaultValue(StatusEnum.Active);

            // Index'ler
            builder.HasIndex(x => x.QrCodeYear)
                .HasDatabaseName("IX_QRCodeBatches_QrCodeYear");

            builder.HasIndex(x => x.CreateTime)
                .HasDatabaseName("IX_QRCodeBatches_CreateTime");

            builder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_QRCodeBatches_Status");
        }
    }
}
