using BankingApp.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Persistence.EntitiesConfigurations
{
    public class AccountEntityConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.UserId).IsRequired();
            builder.Property(x => x.Balance).IsRequired().HasPrecision(18,2);
            builder.Property(x => x.Type).IsRequired();
            builder.Property(x => x.Number).IsRequired();
            builder.Property(x => x.Status).IsRequired();
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.AdminId);

            builder.HasIndex(a => a.Number)
            .IsUnique();

            builder.HasMany(x => x.Transactions)
                    .WithOne(x => x.Account)
                    .HasForeignKey(x => x.AccountNumber)
                    .HasPrincipalKey(x => x.Number);


        }
    }
}
