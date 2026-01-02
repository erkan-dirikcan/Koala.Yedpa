using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations;

public class BudgetRatioConfiguration : IEntityTypeConfiguration<BudgetRatio>
{
    public void Configure(EntityTypeBuilder<BudgetRatio> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Year)
            .IsRequired();

        builder.Property(x => x.Ratio)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.TotalBugget)
            .HasColumnType("decimal(18,2)");

        // Index for performance
        builder.HasIndex(x => new { x.Code, x.Year })
            .IsUnique();

        builder.HasIndex(x => x.Year);
        builder.HasIndex(x => x.BuggetType);
    }
}





