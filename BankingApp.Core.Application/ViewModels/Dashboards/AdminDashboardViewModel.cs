

namespace BankingApp.Core.Application.ViewModels.Dashboards
{
    public class AdminDashboardViewModel
    {
        public required int TotalTransactionsCount { get; set; }
        public required int DayPaysCount { get; set; }
        public required int TotalPaysCount { get; set; }
        public required int TotalInactiveClientsCount { get; set; }
        public required int TotalActiveClientsCount { get; set; }
        public required int TotalAsignedProductsCount { get; set; }
        public required int TotalCurrentLoansCount { get; set; }
        public required int TotalActiveCreditCardsCount { get; set; }
        public required int TotalSavingAccountsCount { get; set; }
        public required decimal AverageClientsDebt { get; set; }
        public required int TotalIssuedCreditCardsCount { get; set; }
        public required int TotalClientCreditCardsCount { get; set; }



    }
}
