using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface ILoanRepository : IGenericRepository<Loan>
    {
        Task<decimal> GetActiveClientsLoanDebt(HashSet<string> ids);
        Task<int> GetActiveLoansCount();
        Task<int> GetAllLoansCount();
        Task<bool> LoanPublicIdExists(string id);
        Task<Loan> UpdateByObjectAsync(Loan entity);
        Task<List<Loan>> GetLoanListByIdClient(string idCliente);
        Task<Loan?> PayLoan( Guid idLoan, decimal amount,int value);
        Task<Loan?> GetLoanByPublicId( string publicId);
       

    }
}
