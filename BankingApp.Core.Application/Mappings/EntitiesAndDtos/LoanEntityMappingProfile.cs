using AutoMapper;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class LoanEntityMappingProfile: Profile

    {
        public LoanEntityMappingProfile()
        {
            CreateMap<Loan, LoanDto>()
                .ForMember(r => r.TotalInstallmentsCount, opt => opt.MapFrom(src => src.Installments.Count()))
           .ForMember(r => r.PaidInstallmentsCount, opt => opt.MapFrom(src => src.Installments.Where(r => r.IsPaid).Count()));

        }
    }
}
