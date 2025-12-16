using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations;

public class ExtendedPropertyRecordValuesConfiguration:IEntityTypeConfiguration<ExtendedPropertyRecordValues>
{
    public void Configure(EntityTypeBuilder<ExtendedPropertyRecordValues> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasOne(x => x.ExtendedProperty)
            .WithMany(x => x.RecordValues)
            .HasForeignKey(x => x.ExtendedPropertyId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}