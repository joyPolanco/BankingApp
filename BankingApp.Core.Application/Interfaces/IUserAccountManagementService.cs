using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.User;


namespace BankingApp.Core.Application.Interfaces
{
    public interface IUserAccountManagementService
    {
        public Task ChangeBalanceForClient(string id, decimal AdditionalBalance);
        Task<RegisterUserWithAccountResponseDto> CreateUserWithAmount(CreateUserDto request, string AdminId, bool ForApi= false,string ? origin = null);
        Task<RegisterUserWithAccountResponseDto> EditUserAndAmountAsync(UpdateUserDto request, string AdminId,bool ForApi= false, string? origin = null);
        Task<AccountDto?> GetMainSavingAccount(string clientId);
    }
}
