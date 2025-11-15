using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;


namespace BankingApp.Core.Application.Interfaces
{
    public interface IInstallmentRepository : IGenericRepository<Installment>
    {


        Task<List<Installment>> GetListInstallamentByLoanId(Guid loanID);
        Task<Installment?> UpdateInstallmentOnPaymentAsync(int id, Installment installment, decimal amount);
        Task<Installment?> GetByIdLoan(Guid loanId);
    }

}
