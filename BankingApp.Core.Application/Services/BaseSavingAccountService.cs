using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankingApp.Core.Application.Services
{
    public abstract class BaseSavingAccountService :GenericService<Account,AccountDto>, IBaseSavingAccountService
    {
        private readonly IMapper _mapper;
        private readonly IAccountRepository _repo;

        public BaseSavingAccountService(IAccountRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<AccountDto?> GetAccountByClientId(string clientId)
        {
            var entity = await _repo.GetAllQuery().Where(r => r.UserId == clientId).FirstOrDefaultAsync();
            if (entity == null) return default;
            return _mapper.Map<AccountDto>(entity);
        }
        public async Task<string> GenerateAccountNumber()
        {
            bool accountExists = false;
            string accountNumber;
            do
            {
                accountNumber = new string(
                         Guid.NewGuid()
                         .ToString("N")
                         .Where(char.IsDigit)
                         .Take(9)
                         .ToArray());
                accountExists = await _repo.AccountExists(accountNumber);

            } while (accountExists);

            return accountNumber;
        }

    }
}
