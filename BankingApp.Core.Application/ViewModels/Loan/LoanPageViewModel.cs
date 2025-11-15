using BankingApp.Core.Domain.Common.Enums;

namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class LoanPageViewModel
    {

        public required List<LoanViewModel> Loans { get; set; }
        public int Page { get; set; }
        public int CurrentPage { get; set; }

        public string? FilterDocumentId { get; set; }

        public LoanStatus? FilterStatus { get; set; }
        public int MaxPages { get; set; }

    }
}