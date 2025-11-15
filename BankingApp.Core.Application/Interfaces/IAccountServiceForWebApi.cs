using BankingApp.Core.Application.Dtos.Login;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IAccountServiceForWebApi : IBaseAccountService
    {
        Task<LoginResponseForApi> AuthenticateAsync(LoginDto loginDto);
        Task DeactivateUsersAsync(List<string> userIds);
    }
}