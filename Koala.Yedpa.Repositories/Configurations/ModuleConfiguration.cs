using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations
{
    public class ModuleConfiguration : IEntityTypeConfiguration<Module>
    {
        public void Configure(EntityTypeBuilder<Module> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasMany(x => x.GeneratedIds)
                .WithOne(x => x.Module)
                .HasForeignKey(x => x.ModuleId);

            builder.HasMany(x => x.Claims)
                .WithOne(x => x.Module)
                .HasForeignKey(x => x.ModuleId);

            builder.HasMany(x => x.ExtendedProperties)
                .WithOne(x => x.Module)
                .HasForeignKey(x => x.ModuleId);
        }
    }
}
