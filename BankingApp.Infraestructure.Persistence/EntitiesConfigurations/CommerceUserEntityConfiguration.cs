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
    public class CommerceUserEntityConfiguration : IEntityTypeConfiguration<CommerceUser>
    {
        public void Configure(EntityTypeBuilder<CommerceUser> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x=>x.UserId).IsRequired();
            builder.Property(x => x.CommerceId).IsRequired();
            builder.HasIndex(x=>new { x.UserId,x.CommerceId}).IsUnique();

        }
    }
}
