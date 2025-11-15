using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class InstallmentRepository : GenericRepository<Installment>, IInstallmentRepository
    {

        private readonly BankingContext context;


        public InstallmentRepository(BankingContext context) : base(context)
        {

            this.context = context;

        }

        public async Task<Installment?> GetByIdLoan(Guid loanId)
        {

            return context.Set<Installment>().FirstOrDefault(s => s.LoanId == loanId);
          
        }

        public async Task<List<Installment>> GetListInstallamentByLoanId(Guid loanID)
        {

            return await context.Set<Installment>().Where(s => s.LoanId == loanID && s.IsPaid == false).ToListAsync();

        }

        public async Task<Installment?> UpdateInstallmentOnPaymentAsync(int id, Installment installment, decimal amount)
        {

            var entity = await context.Set<Installment>().FindAsync(id);

            if (entity is not null)
            {

                if (entity.Value <= amount)
                {
                    entity.IsPaid = true;
                }
                else
                {
                    entity.IsPaid = false;
                    entity.Value -= amount; 
                }

                entity.IsModified = false;
                installment.IsDelinquent = installment.PayDate < DateOnly.FromDateTime(DateTime.Now);
                await context.SaveChangesAsync();
                return entity;

            }


            return null;

        }
    }
}
