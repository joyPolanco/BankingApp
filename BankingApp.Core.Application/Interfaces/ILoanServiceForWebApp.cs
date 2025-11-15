using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ILoanServiceForWebApp : IGenericService<Loan, LoanDto>, IBaseLoanService
    {
        Task<List<UserDto>> GetClientsAvailableForLoan(string? DocumentId = null);

        Task<CreateLoanResult> HandleCreateRequest(LoanRequest request);

    }
}
