using BankingApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankingApp.Infraestructure.Persistence.EntitiesConfigurations
{
    public class CommerceEntityConfiguration : IEntityTypeConfiguration<Commerce>
    {
        public void Configure(EntityTypeBuilder<Commerce> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired();
            builder.Property(x => x.Description);
            builder.Property(x => x.IsActive).IsRequired();
            builder.Property(x => x.Logo).IsRequired();

            builder.HasMany(c => c.Users)
         .WithOne(u => u.Commerce)
         .HasForeignKey(u => u.CommerceId)
         .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
