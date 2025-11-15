using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Loan
{
    public class CreateLoanResult
    {
        public bool ClientIsHighRisk {  get; set; }
        public bool ClientHasActiveLoan { get; set; }
        public bool LoanCreated { get; internal set; }
        public bool ClientIsAlreadyHighRisk { get; set; }
        public string ValidationMessage { get; internal set; }
        public bool HasValidationErrors { get; internal set; }
    }
}
