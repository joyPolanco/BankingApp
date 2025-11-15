using BankingApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BankingApp.Infraestructure.Persistence.EntitiesConfigurations
{
    public class TransactionEntityConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x=>x.Status).IsRequired();
            builder.Property(x => x.Origin).IsRequired();
            builder.Property(x => x.Beneficiary).IsRequired();
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Amount).IsRequired().HasPrecision(18, 2);
            builder.Property(x => x.DateTime).IsRequired();
            builder.Property(x => x.AccountNumber).IsRequired();
            builder.Property(x => x.AccountId);
            builder.Property(x => x.Description).IsRequired().HasMaxLength(100);

        }
    }
}
