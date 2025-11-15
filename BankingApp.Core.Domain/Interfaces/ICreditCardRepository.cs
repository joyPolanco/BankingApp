using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Domain.Interfaces
{
    public interface ICreditCardRepository : IGenericRepository<CreditCard>
    {
        Task<CreditCard?> GetByNumberAsync(string cardNumber);
        Task<List<CreditCard>> GetByClientIdAsync(string clientId);
        Task<bool> CardNumberExistsAsync(string cardNumber);

        Task<CreditCard?> DebitTotalAmountOwedAsync(string number, decimal Amount);
        Task<List<CreditCard>> GetActiveByClientIdAsync(string clientId);


        Task<int> GetTotalActiveCreditCards();
        Task<int> GetTotalIssuedCreditCards();
        Task<int> GetTotalActiveCreditCardsWithClient();
        Task<int> GetTotalCreditCardsWithClient();
        Task<decimal> GetActiveClientsCreditCardDebt(HashSet<string> ids);
        Task<decimal> GetTotalClientsCreditCardDebt();
        Task<decimal> GetClientTotalCreditCardDebt(string ClientId);
    }
}
