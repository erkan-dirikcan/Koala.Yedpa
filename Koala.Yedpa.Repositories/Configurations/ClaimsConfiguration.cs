using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations
{
    public class ClaimsConfiguration : IEntityTypeConfiguration<Claims>
    {
        public void Configure(EntityTypeBuilder<Claims> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(x => x.Module)
                .WithMany(x => x.Claims)
                .HasForeignKey(x => x.ModuleId);

            // Claims.Name doğrudan authorization policy adı olarak kullanılıyor;
            // benzersizliği DB seviyesinde garanti altına alıyoruz.
            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            builder.HasIndex(x => x.Name)
                .IsUnique()
                .HasDatabaseName("IX_Claims_Name_Unique");
        }
    }
}
