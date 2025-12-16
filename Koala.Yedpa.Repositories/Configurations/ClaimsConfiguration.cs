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

        }
    }
}
