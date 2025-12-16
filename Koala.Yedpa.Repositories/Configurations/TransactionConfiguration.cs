using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);
        
        builder.HasMany(x=>x.TransactionItems)
            .WithOne(x=>x.Transaction)
            .HasForeignKey(x=>x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x=>x.TransactionType)
            .WithMany(x=>x.Transactions)
            .HasForeignKey(x=>x.TransactionTypeId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.AppUser)
            .WithMany(x => x.Transactions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        

    }
}