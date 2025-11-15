using BankingApp.Core.Application.ViewModels.Installment;


namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class DetailedLoanViewModel
    {

        public required string LoadId { get; set; }

        public required List<InstallmentViewModel> Installments { get; set; }
    }
}
