using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations;

public class TransactionTypeConfiguration : IEntityTypeConfiguration<TransactionType>
{
    public void Configure(EntityTypeBuilder<TransactionType> builder)
    {
        builder.HasKey(x => x.Id);
        builder.HasMany(x => x.Transactions)
            .WithOne(x => x.TransactionType)
            .HasForeignKey(x => x.TransactionTypeId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}