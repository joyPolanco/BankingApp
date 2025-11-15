using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class CommerceUserRepository : GenericRepository<CommerceUser>, ICommerceUserRepository
    {
        public CommerceUserRepository(BankingContext context) : base(context)
        {
        }
    }
}
