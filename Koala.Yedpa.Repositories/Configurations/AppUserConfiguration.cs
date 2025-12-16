using Koala.Yedpa.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Koala.Yedpa.Repositories.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {

            builder.HasMany(x => x.Transactions)
                .WithOne(x => x.AppUser)
                .HasForeignKey(x => x.UserId);

        }
    }
}
