using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations
{
    public class ExtendedPropertyValuesConfiguration:IEntityTypeConfiguration<ExtendedPropertyValues>
    {
        public void Configure(EntityTypeBuilder<ExtendedPropertyValues> builder)
        {
            builder.HasKey(e => e.Id);
            
            builder.HasOne(x => x.ExtendedProperty)
                .WithMany(x => x.Values)
                .HasForeignKey(x => x.ExtendedPropertyId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
