using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.Dtos
{
    public class AccountMappingsProfile: Profile
    {
        public AccountMappingsProfile()
        {
            CreateMap<AccountDto, PrimaryAccountDto>();
        }
    }
}
