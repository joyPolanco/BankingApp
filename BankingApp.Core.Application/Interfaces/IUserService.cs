using BankingApp.Core.Application.Dtos.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserPaginationResultDto> GetAllExceptCommerce(int page = 1, int pageSize = 20, string? rol = null);
        Task<UserPaginationResultDto> GetAllOnlyCommerce(int page = 1, int pageSize = 20, string? rol = null);
        Task<UserDto?> GetByDocumentId(string documentId);
        Task<UserDto?> GetUserById(string userId);
        Task<List<string>> GetActiveUserIdsAsync(); // Nuevo método para obtener IDs de usuarios activos
        Task ToogleState(string userId);
        Task<List<UserDto>> GetClientsWithDebtInfo(Dictionary<string, decimal> clientDebts, string? documentId);
        Task<UserDto?> GetCurrentUserAsync();
        Task<int> GetActiveClientsCount();
        Task<int> GetInactiveClientsCount();
        Task<HashSet<string>> GetAllClientIds();
        Task<HashSet<string>> GetActiveClientsIds();
    }
}
