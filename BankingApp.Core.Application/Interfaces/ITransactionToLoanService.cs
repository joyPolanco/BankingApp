

using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Transaction;
using System.Transactions;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ITransactionToLoanService : IGenericService<Transaction,CreateTransactionDto>
    {


        Task<List<string>> GetLoanActive(string idCliente);
        Task<LoanDto?> GetLoanBypublicIdAsync(string publicId);
        Task<PayLoanResponseDto?> PayLoanAsync(Guid LoanId, decimal amount);




    }
}
