using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Infraestructure.Persistence.Repositories
{
    public class LoanRepository : GenericRepository<Loan>, ILoanRepository
    {

        private readonly BankingContext _bankingContext;
        public LoanRepository(BankingContext context) : base(context)
        {
            _bankingContext = context;
        }

        public async Task<bool> LoanPublicIdExists(string id)
        {
            return await _bankingContext.Set<Loan>().AnyAsync(r => r.PublicId == id);

        }

        public async Task<Loan> UpdateByObjectAsync(Loan entity)
        {
            var entry = await _bankingContext.Set<Loan>().Where(r => r.Id == entity.Id).FirstOrDefaultAsync();

            if (entry != null)
            {
                _bankingContext.Entry(entry).CurrentValues.SetValues(entity);
                await _bankingContext.SaveChangesAsync();
            }
            return entry;
        }


        public async Task<List<Loan>> GetLoanListByIdClient(string idCliente)
        {

            return await _bankingContext.Set<Loan>().Where(l => l.ClientId == idCliente && l.IsActive == true).ToListAsync();

        }


        public async Task<Loan?> PayLoan(Guid idLoan, decimal amount, int value)
        {
            var loan = await _bankingContext.Set<Loan>().FirstOrDefaultAsync(c => c.Id == idLoan);

            if (loan != null || loan!.OutstandingBalance > 0)
            {

                loan.OutstandingBalance -= amount;
                loan.UpdatedAt = DateTime.UtcNow;

                if (value > 1)
                {
                    loan.IsActive = true;
                }
                else
                {

                    loan.IsActive = false;

                }

                await _bankingContext.SaveChangesAsync();
                return loan;
            }

            return null;
        }



        public  async Task<Loan?> GetLoanByPublicId(string publicId)
        {

           return  await _bankingContext.Set<Loan>().FirstOrDefaultAsync(c => c.PublicId == publicId);

        }

        public async Task<int> GetActiveLoansCount()
        {
           return await _bankingContext.Set<Loan>().Where(r=>r.IsActive && r.ClientId!=null).CountAsync();
        }
        public async Task<int> GetAllLoansCount()
        {
            return await _bankingContext.Set<Loan>().Where(r => r.ClientId != null).CountAsync();
        }

        public async Task<decimal> GetActiveClientsLoanDebt(HashSet<string> ids)
        {
           return await _bankingContext.Set<Loan>()
                 .Where(r => r.IsActive && ids.Contains(r.ClientId)).
                 SumAsync(l => l.OutstandingBalance);
        }

    }
}
