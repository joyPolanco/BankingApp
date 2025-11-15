using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;


namespace BankingApp.Core.Application.Services
{
    public class SavingAccountServiceForApi : BaseSavingAccountService, ISavingAccountServiceForApi
    {

        private readonly IAccountRepository _repo;
        private readonly IMapper _mapper;
        public SavingAccountServiceForApi(IAccountRepository repo, IMapper mapper) : base(repo, mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

    
    }
}
