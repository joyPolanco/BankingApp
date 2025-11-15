using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.SavingsAccount;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Core.Application.Services
{
    public class SavingsAccountServiceForWebApp :BaseSavingAccountService, ISavingsAccountServiceForWebApp
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public SavingsAccountServiceForWebApp(
            IAccountRepository accountRepository,
            IUserService userService,
            IMapper mapper):base(accountRepository,mapper)
        {
            _accountRepository = accountRepository;
            _userService = userService;
            _mapper = mapper;
        }

        public async Task<List<AccountDto>> GetAllAccountsAsync(int page, int pageSize, string? cedula = null, string? estado = null, string? tipo = null)
        {
            var accounts = await _accountRepository.GetAllList();
            var accountsList = accounts?.ToList() ?? new List<Account>();

            // Obtener todos los usuarios activos para filtrar las cuentas
            var activeUserIds = await _userService.GetActiveUserIdsAsync();

            // Filtrar solo cuentas de usuarios activos
            accountsList = accountsList.Where(a => activeUserIds.Contains(a.UserId)).ToList();

            // Filtrar por cédula si se proporciona
            if (!string.IsNullOrEmpty(cedula))
            {
                cedula = cedula.Trim().Replace("-", "");
                var user = await _userService.GetByDocumentId(cedula);
                if (user != null)
                {
                    accountsList = accountsList.Where(a => a.UserId == user.Id).ToList();
                }
                else
                {
                    return new List<AccountDto>();
                }
            }

            // Filtrar por estado
            if (!string.IsNullOrEmpty(estado))
            {
                if (Enum.TryParse<AccountStatus>(estado, out var statusEnum))
                {
                    accountsList = accountsList.Where(a => a.Status == statusEnum).ToList();
                }
            }
            else
            {
                // Si NO se especifica filtro de estado, por defecto solo mostrar cuentas ACTIVAS
                // Las cuentas CANCELADAS no aparecen en el listado
                accountsList = accountsList.Where(a => a.Status == AccountStatus.ACTIVE).ToList();
            }

            // Filtrar por tipo
            if (!string.IsNullOrEmpty(tipo))
            {
                if (Enum.TryParse<AccountType>(tipo, out var typeEnum))
                {
                    accountsList = accountsList.Where(a => a.Type == typeEnum).ToList();
                }
            }

            // Ordenar: primero activas, luego canceladas, siempre de más reciente a más antigua
            var sortedAccounts = accountsList
                .OrderByDescending(a => a.Status == AccountStatus.ACTIVE)
                .ThenByDescending(a => a.CreatedAt)
                .ToList();

            return _mapper.Map<List<AccountDto>>(sortedAccounts);
        }

        public async Task<AccountDto?> GetAccountByIdAsync(int id)
        {
            var account = await _accountRepository.GetByIdAsync(id);
            return account != null ? _mapper.Map<AccountDto>(account) : null;
        }

        public async Task<AccountDto?> GetAccountByNumberAsync(string accountNumber)
        {
            var accounts = await _accountRepository.GetAllList();
            var account = accounts?.FirstOrDefault(a => a.Number == accountNumber);
            return account != null ? _mapper.Map<AccountDto>(account) : null;
        }

        public async Task<List<AccountDto>> GetAccountsByClientIdAsync(string clientId)
        {
            var accounts = await _accountRepository.GetAllList();
            var clientAccounts = accounts?.Where(a => a.UserId == clientId).ToList() ?? new List<Account>();
            return _mapper.Map<List<AccountDto>>(clientAccounts);
        }

        public async Task<AccountDto?> GetPrimaryAccountByClientIdAsync(string clientId)
        {
            var accounts = await _accountRepository.GetAllList();
            var primaryAccount = accounts?.FirstOrDefault(a => 
                a.UserId == clientId && 
                a.Type == AccountType.PRIMARY &&
                a.Status == AccountStatus.ACTIVE);
            
            return primaryAccount != null ? _mapper.Map<AccountDto>(primaryAccount) : null;
        }

        public async Task<AccountDto> CreateSecondaryAccountAsync(AccountDto accountDto, string adminId)
        {
            // Generar número de cuenta único
            var accountNumber = await GenerateAccountNumber();
            
            var account = _mapper.Map<Account>(accountDto);
            account.Number = accountNumber;
            account.Type = AccountType.SECONDARY;
            account.Status = AccountStatus.ACTIVE;
            account.CreatedAt = DateTime.Now;
            account.AdminId = adminId;
            account.Balance = accountDto.Balance;

            var createdAccount = await _accountRepository.AddAsync(account);
            return _mapper.Map<AccountDto>(createdAccount);
        }

        public async Task<bool> CancelAccountAsync(int accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            
            if (account == null || account.Type == AccountType.PRIMARY)
                return false;

            // Si tiene balance, transferir a cuenta principal
            if (account.Balance > 0)
            {
                var primaryAccount = await GetPrimaryAccountByClientIdAsync(account.UserId);
                if (primaryAccount != null)
                {
                    // Transferir fondos
                    var primaryAccountEntity = await _accountRepository.GetByIdAsync(primaryAccount.Id);
                    if (primaryAccountEntity != null)
                    {
                        primaryAccountEntity.Balance += account.Balance;
                        await _accountRepository.UpdateAsync(primaryAccount.Id, primaryAccountEntity);
                    }
                    
                    // Dejar en cero la cuenta a cancelar
                    account.Balance = 0;
                }
            }

            // Marcar como cancelada
            account.Status = AccountStatus.CANCELLED;
            await _accountRepository.UpdateAsync(accountId, account);

            return true;
        }

        public async Task<int> GetTotalAccountsCountAsync(string? cedula = null, string? estado = null, string? tipo = null)
        {
            var accounts = await GetAllAccountsAsync(1, int.MaxValue, cedula, estado, tipo);
            return accounts.Count;
        }

        public async Task<List<TransactionViewModel>> GetAccountTransactionsAsync(int accountId)
        {
            var account = await _accountRepository.GetByIdAsync(accountId);
            
            if (account == null)
                return new List<TransactionViewModel>();

            // Obtener las cuentas con sus transacciones incluidas
            var accounts = await _accountRepository.GetAllListWithInclude(new List<string> { "Transactions" });
            var accountWithTransactions = accounts?.FirstOrDefault(a => a.Id == accountId);
            
            if (accountWithTransactions?.Transactions == null || !accountWithTransactions.Transactions.Any())
                return new List<TransactionViewModel>();

            // Mapear y ordenar transacciones por fecha (más reciente primero)
            var transactions = _mapper.Map<List<TransactionViewModel>>(accountWithTransactions.Transactions);
            return transactions.OrderByDescending(t => t.TransactionDate).ToList();
        }
    }
}
