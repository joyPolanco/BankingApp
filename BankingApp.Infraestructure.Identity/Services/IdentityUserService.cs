using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Contexts;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;


namespace BankingApp.Infraestructure.Identity.Services
{
    public class IdentityUserService : IUserService
    {

        private UserManager<AppUser> _userManager;
        private readonly IdentityContext _identityContext;
        private readonly IHttpContextAccessor _context;

        public IdentityUserService(UserManager<AppUser> userManager, IdentityContext identityDbContext, IHttpContextAccessor context)
        {
            _userManager = userManager;
            _identityContext = identityDbContext;
            _context = context;

        }


        public async Task<UserDto?> GetByDocumentId(string documentId)
        {
            // Limpiar espacios en blanco y normalizar (eliminar guiones y espacios)
            var cleanDocumentId = documentId?.Trim().Replace("-", "").Replace(" ", "") ?? "";

            // Buscar con diferentes formatos posibles
            var user = await _userManager.Users
                .Where(r => r.DocumentIdNumber.Replace("-", "").Replace(" ", "") == cleanDocumentId)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                DocumentIdNumber = user.DocumentIdNumber,
                IsVerified = user.EmailConfirmed,
                Status = user.IsActive ? "Activo" : "Inactivo",
                IsActive = user.IsActive,
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }

        public async Task<UserDto?> GetUserById(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                DocumentIdNumber = user.DocumentIdNumber,
                IsVerified = user.EmailConfirmed,
                Status = user.IsActive ? "Activo" : "Inactivo",
                IsActive = user.IsActive,
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }

        public async Task<UserDto?> GetById(string Id)
        {
            var user = await _userManager.Users.Where(r => r.Id == Id).FirstOrDefaultAsync();

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                DocumentIdNumber = user.DocumentIdNumber,
                IsVerified = user.EmailConfirmed,
                Status = user.IsActive ? "Activo" : "Inactivo",
                IsActive = user.IsActive,
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }

        public async Task<UserPaginationResultDto> GetAllOnlyCommerce(int page = 1, int pageSize = 20, string? rol = null)
        {
            var commerceRoleId = await _identityContext.Roles
                .Where(r => r.Name == AppRoles.COMMERCE.ToString())
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            string? extraRoleId = null;
            if (!string.IsNullOrWhiteSpace(rol))
            {
                try
                {
                    var enumValue = EnumMapper<AppRoles>.FromString(rol);
                    var enumName = enumValue.ToString();

                    extraRoleId = await _identityContext.Roles
                        .Where(r => r.Name.Equals(enumName, StringComparison.OrdinalIgnoreCase))
                        .Select(r => r.Id)
                        .FirstOrDefaultAsync();
                }
                catch
                {
                    extraRoleId = null;
                }
            }

            var query = from ur in _identityContext.UserRoles
                        where ur.RoleId == commerceRoleId
                        join u in _identityContext.Users on ur.UserId equals u.Id
                        select u;
            var totalCount = await query.CountAsync();

            if (extraRoleId != null)
            {
                var extraUserIds = await _identityContext.UserRoles
                    .Where(ur => ur.RoleId == extraRoleId)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                query = query.Where(u => extraUserIds.Contains(u.Id));
            }


            var pagedUsers = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userIds = pagedUsers.Select(u => u.Id).ToList();

            var roles = await _identityContext.UserRoles
                .Where(ur => userIds.Contains(ur.UserId) && ur.RoleId != commerceRoleId)
                .Join(_identityContext.Roles,
                      ur => ur.RoleId,
                      r => r.Id,
                      (ur, r) => new { ur.UserId, RoleName = r.Name })
                .GroupBy(x => x.UserId)
                .Select(g => new { UserId = g.Key, RoleName = g.Select(x => x.RoleName).FirstOrDefault() })
                .ToListAsync();

            var roleMap = roles.ToDictionary(x => x.UserId, x => x.RoleName);

            var result = pagedUsers.Select(u =>
            {
                var roleName = roleMap.GetValueOrDefault(u.Id) ?? AppRoles.COMMERCE.ToString();
                string displayRole;

                try
                {
                    var enumValue = EnumMapper<AppRoles>.FromString(roleName);
                    displayRole = EnumMapper<AppRoles>.ToString(enumValue);
                }
                catch
                {
                    displayRole = roleName;
                }

                return new UserDto
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    Name = u.Name,
                    LastName = u.LastName,
                    Role = displayRole,
                    DocumentIdNumber = u.DocumentIdNumber,
                    UserName = u.UserName ?? "",
                    Status = u.EmailConfirmed ? "activo" : "inactivo"
                };
            }).ToList();

            return new UserPaginationResultDto
            {
                Data = result,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
                TotalCount = totalCount
            };
        }


        public async Task<UserPaginationResultDto> GetAllExceptCommerce(
       int page = 1,
       int pageSize = 20,
       string? rol = null)
        {
            var commerceRoleId = await _identityContext.Roles
                .Where(r => r.Name == AppRoles.COMMERCE.ToString())
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            string? extraRoleId = null;

            if (!string.IsNullOrWhiteSpace(rol))
            {
                try
                {
                    var enumValue = EnumMapper<AppRoles>.FromString(rol);
                    var enumName = enumValue.ToString();

                    extraRoleId = await _identityContext.Roles
                        .Where(r => r.Name == enumName)
                        .Select(r => r.Id)
                        .FirstOrDefaultAsync();
                }
                catch
                {
                    extraRoleId = null;
                }
            }

            var query = from ur in _identityContext.UserRoles
                        join u in _identityContext.Users on ur.UserId equals u.Id
                        join r in _identityContext.Roles on ur.RoleId equals r.Id
                        where ur.RoleId != commerceRoleId
                        select new { User = u, Role = r };

            if (!string.IsNullOrEmpty(extraRoleId))
            {
                query = query.Where(x => x.Role.Id == extraRoleId);
            }

            var totalCount = await query.CountAsync();

            var pagedUsers = await query
                .OrderByDescending(x => x.User.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = pagedUsers.Select(x =>
            {
                string displayRole;

                try
                {
                    var enumValue = EnumMapper<AppRoles>.FromString(x.Role.Name ?? "");
                    displayRole = EnumMapper<AppRoles>.ToString(enumValue);
                }
                catch
                {
                    displayRole = x.Role.Name ?? "";
                }

                return new UserDto
                {
                    Id = x.User.Id,
                    Email = x.User.Email ?? "",
                    Name = x.User.Name,
                    LastName = x.User.LastName,
                    Role = displayRole,
                    DocumentIdNumber = x.User.DocumentIdNumber,
                    UserName = x.User.UserName ?? "",
                    IsActive = x.User.IsActive,
                    IsVerified = x.User.EmailConfirmed,
                    Status = x.User.IsActive ? "activo" : "inactivo"
                };
            }).ToList();

            var PagesCount = (int)Math.Ceiling(totalCount / (double)pageSize);
            return new UserPaginationResultDto
            {
                Data = result,
                PagesCount = PagesCount,
                CurrentPage = PagesCount==0? 0 :page,
                TotalCount = totalCount
            };
        }


        public async Task<List<string>> GetActiveUserIdsAsync()
        {
            var activeUserIds = await _userManager.Users
                .Where(u => u.IsActive)
                .Select(u => u.Id)
                .ToListAsync();

            return activeUserIds;
        }

        public async Task ToogleState(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.IsActive = !user.IsActive;

            await _userManager.UpdateAsync(user);

        }

        public async Task<List<UserDto>> GetClientsWithDebtInfo(Dictionary<string, decimal> clientDebts, string? DocumentId = null)
        {
            var clientRole = await _identityContext.Roles
                .FirstOrDefaultAsync(r => r.Name == AppRoles.CLIENT.ToString());

            if (clientRole == null)
                return new List<UserDto>();

            var userRoles = await _identityContext.UserRoles
                .Where(r => r.RoleId == clientRole.Id)
                .ToListAsync();

            var usersId = userRoles
                .Where(r => !clientDebts.ContainsKey(r.UserId))
                .Select(r => r.UserId)
                .Distinct()
                .ToList();

            var query = _userManager.Users
                .Where(u => u.IsActive && usersId.Contains(u.Id));

            if (!string.IsNullOrEmpty(DocumentId))
            {
                query = query.Where(r => r.DocumentIdNumber == DocumentId);
            }

            var users = await query
                .Select(u => new UserDto
                {
                    DocumentIdNumber = u.DocumentIdNumber,
                    Email = u.Email,
                    Id = u.Id,
                    LastName = u.LastName,
                    Name = u.Name,
                    Role = "",
                    Status = u.IsActive ? "Activo" : "Inactivo",
                    UserName = u.UserName,
                    IsActive = u.IsActive,
                    TotalDebt = clientDebts.ContainsKey(u.Id) ? clientDebts[u.Id] : 0m
                })
                .ToListAsync();

            return users;
        }



        public async Task<UserDto?> GetCurrentUserAsync()
        {
            var httpUser = _context.HttpContext?.User;
            if (httpUser?.Identity == null || !httpUser.Identity.IsAuthenticated)
                return null;

          

            var user = await _userManager.Users.Where(r=>r.UserName==httpUser.Identity.Name).FirstOrDefaultAsync();
            if (user == null)
                return null;

            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                LastName = user.LastName,
                DocumentIdNumber = user.DocumentIdNumber,
                Email = user.Email,
                UserName = user.UserName,
                Status= user.IsActive? "Activo": "Inactivo",
                Role=""
            };
        }


        public async Task<int> GetActiveClientsCount()
        {
            var clientRoleId = await _identityContext.Roles
        .Where(r => r.Name.ToLower() == AppRoles.CLIENT.ToString().ToLower())
        .Select(r => r.Id)
        .FirstOrDefaultAsync();

            return await _identityContext.UserRoles
                .Where(ur => ur.RoleId == clientRoleId)
                .Join(
                    _identityContext.Users,
                    ur => ur.UserId,
                    u => u.Id,
                    (ur, u) => u
                )
                .Where(u => u.IsActive)
                .Distinct()
                .CountAsync();
        }
        public async Task<int> GetInactiveClientsCount()
        {
            var clientRoleId = await _identityContext.Roles
        .Where(r => r.Name.ToLower() == AppRoles.CLIENT.ToString().ToLower())
        .Select(r => r.Id)
        .FirstOrDefaultAsync();

            return await _identityContext.UserRoles
                .Where(ur => ur.RoleId == clientRoleId)
                .Join(
                    _identityContext.Users,
                    ur => ur.UserId,
                    u => u.Id,
                    (ur, u) => u
                )
                .Where(u => !u.IsActive)
                .Distinct()
                .CountAsync();
        }


        public async Task<HashSet<string>> GetAllClientIds()
        {
            var clientRoleId = await _identityContext.Roles
                .Where(r => r.Name == AppRoles.CLIENT.ToString())
                .Select(r => r.Id)
                .FirstOrDefaultAsync();

            return (await _identityContext.UserRoles
                .Where(ur => ur.RoleId == clientRoleId)
                .Select(ur => ur.UserId)
                .ToListAsync())
                .ToHashSet(); 
        }
        public async Task<HashSet<string>>  GetActiveClientsIds()
        {
            var clientRoleId = await _identityContext.Roles
        .Where(r => r.Name.ToLower() == AppRoles.CLIENT.ToString().ToLower())
        .Select(r => r.Id)
        .FirstOrDefaultAsync();

            return await _identityContext.UserRoles
                .Where(ur => ur.RoleId == clientRoleId)
                .Join(
                    _identityContext.Users,
                    ur => ur.UserId,
                    u => u.Id,
                    (ur, u) => u
                )
                .Where(u => u.IsActive)
                .Distinct()
                .Select(u=>u.Id).ToHashSetAsync();
        }

    }
}
