using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations;

public class LgXt001211Configuration : IEntityTypeConfiguration<LgXt001211>
{
    public void Configure(EntityTypeBuilder<LgXt001211> builder)
    {
        builder.HasKey(e => e.Id); // Senin anahtar
        builder.Property(e => e.Id).ValueGeneratedNever(); // Manuel Guid

        builder.Property(e => e.LogRef)
            .HasColumnName("LOGREF")
            .ValueGeneratedNever(); // Identity değil
     
    }
}