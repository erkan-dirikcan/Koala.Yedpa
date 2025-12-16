using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations;

public class ExtendedPropertiesConfiguration:IEntityTypeConfiguration<ExtendedProperties>
{
    public void Configure(EntityTypeBuilder<ExtendedProperties> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasMany(x=>x.Values)
            .WithOne(x => x.ExtendedProperty)
            .HasForeignKey(x => x.ExtendedPropertyId)
            .OnDelete(DeleteBehavior.NoAction);
        builder.HasMany(x => x.RecordValues)
            .WithOne(x => x.ExtendedProperty)
            .HasForeignKey(x => x.ExtendedPropertyId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Module)
            .WithMany(x => x.ExtendedProperties)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}