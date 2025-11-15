using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ILoanServiceForWebApi : IBaseLoanService
    {
        Task<CreateLoanResult> HandleCreateRequestApi(LoanRequest request);
    }
}
