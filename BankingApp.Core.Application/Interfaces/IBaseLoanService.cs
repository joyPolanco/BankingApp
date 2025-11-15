using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IBaseLoanService : IGenericService<Loan, LoanDto>
    {
        Task<bool> ClientHasActiveLoan(string clientId);
        Task<LoanPaginationResultDto> GetAllFiltered(int page = 1, int pageSize = 20, string? state = null, string? documentId = null);
        Task<DetailedLoanDto?> GetDetailed(string Id);
        public  Task<string> GenerateLoanId();
        Task VerifyAndMarkDelayedLoansAsync();

        Task<OperationResultDto> UpdateLoanRate(string publicId, decimal newRate);
        Task<decimal> GetTotalLoanDebt();
        Task<CreateLoanResult> Create(LoanRequest request);
        Task<CreateLoanResult> SendEmail(LoanRequest request, CreateLoanResult createLoanResult);
        Task<decimal> GetClientLoansDebt(string clientId);
    }
}
