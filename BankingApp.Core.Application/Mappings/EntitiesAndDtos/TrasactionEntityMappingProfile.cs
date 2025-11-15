
using AutoMapper;
using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class TrasactionEntityMappingProfile : Profile
    {

        public TrasactionEntityMappingProfile()
        {



            CreateMap<Transaction, CreateTransactionDto>()
                .ReverseMap()
                .ForMember(s => s.Account, opt => opt.Ignore());


            CreateMap<ValidateAcountNumberExistResponseDto, ValidateAccountNumberResponseDto>()
                .ForMember(s => s.Name, opt => opt.MapFrom(src => src.NameBeneficiary))
                .ForMember(s => s.BeneficiaryId, opt => opt.MapFrom(src => src.IdBeneficiary))
                .ForMember(s => s.IsExist, opt => opt.MapFrom(src => src.IsExist))
                .ForMember(s => s.LastName, opt => opt.Ignore())
                .ForMember(s => s.Gmail, opt => opt.Ignore());
               

            
        }
        

    }
}
