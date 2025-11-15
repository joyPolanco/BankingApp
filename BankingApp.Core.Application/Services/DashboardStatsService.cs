using BankingApp.Core.Application.Dtos.Stats;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;


namespace BankingApp.Core.Application.Services
{
    public class DashboardStatsService :IDashboardsStatsService
    {
        private readonly IUserService _userService;
        private readonly ILoanRepository _loanRepository;
        private readonly IAccountRepository _accountRepository;
        private readonly ICreditCardRepository _creditCardRepository;

        public DashboardStatsService(
            IUserService userService,
            ILoanRepository loanRepository,
            IAccountRepository accountRepository
            ,

            ICreditCardRepository creditCardRepository
            )
        {
            _userService = userService;
            _loanRepository = loanRepository;
            _accountRepository = accountRepository;
            _creditCardRepository = creditCardRepository;
        }


        public async Task<AdminDashboardStatsDto> GetAdminStats()
        {

            var totalAccounts = await _accountRepository.CountSavingAccountsByUserIds(await _userService.GetAllClientIds());
            var totalActiveClients = await _userService.GetActiveClientsCount();
            return new AdminDashboardStatsDto
            {
                ///fantan los pagos y transacciones

                TotalTransactionsCount = 0,
                DayPaysCount = 0,
                TotalPaysCount = 0,
                TotalActiveClientsCount = await _userService.GetActiveClientsCount(),
                TotalInactiveClientsCount = totalActiveClients,
                TotalAsignedProductsCount = totalAccounts + await _loanRepository.GetAllLoansCount() + await _creditCardRepository.GetTotalCreditCardsWithClient(),
                TotalCurrentLoansCount = await _loanRepository.GetActiveLoansCount(),
                TotalActiveCreditCardsCount = await _creditCardRepository.GetTotalActiveCreditCards(),

                TotalIssuedCreditCardsCount = await _creditCardRepository.GetTotalIssuedCreditCards(),
                TotalClientCreditCardsCount = await _creditCardRepository.GetTotalActiveCreditCardsWithClient(),

                TotalSavingAccountsCount = totalAccounts,

                AverageClientsDebt = await GetActiveClientsDebt()/ totalActiveClients

            };




           
        }


       private async Task<decimal> GetActiveClientsDebt()
        {
            var activeUserIds = await _userService.GetActiveClientsIds();
            if (activeUserIds == null || !activeUserIds.Any())
                return 0;

            var loanDebts= await _loanRepository.GetActiveClientsLoanDebt(activeUserIds);
            var creditCardDebts = await _creditCardRepository.GetActiveClientsCreditCardDebt(activeUserIds);
            return loanDebts + creditCardDebts;
        }


    }
}
