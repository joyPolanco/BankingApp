using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.ViewModels.SavingsAccount;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ISavingsAccountServiceForWebApp: IBaseSavingAccountService
    {
        Task<List<AccountDto>> GetAllAccountsAsync(int page, int pageSize, string? cedula = null, string? estado = null, string? tipo = null);
        Task<AccountDto?> GetAccountByIdAsync(int id);
        Task<AccountDto?> GetAccountByNumberAsync(string accountNumber);
        Task<List<AccountDto>> GetAccountsByClientIdAsync(string clientId);
        Task<AccountDto?> GetPrimaryAccountByClientIdAsync(string clientId);
        Task<AccountDto> CreateSecondaryAccountAsync(AccountDto accountDto, string adminId);
        Task<bool> CancelAccountAsync(int accountId);
        Task<int> GetTotalAccountsCountAsync(string? cedula = null, string? estado = null, string? tipo = null);
        Task<List<TransactionViewModel>> GetAccountTransactionsAsync(int accountId);
    }
}
