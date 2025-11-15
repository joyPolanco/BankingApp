using AutoMapper;
using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Domain.Entities;

namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class BeneficiaryEntityMappingProfile : Profile
    {


        public BeneficiaryEntityMappingProfile()
        {



            CreateMap<Beneficiary, CreateBeneficiaryDto>()
                 .ReverseMap();


            CreateMap<UserDto, DataBeneficiaryDto>()
                .ForMember(s => s.Number, opt => opt.Ignore())
                .ForMember(s => s.Id, opt => opt.Ignore())
                .ForMember(s => s.IdBeneficiary, opt => opt.Ignore())
                .ReverseMap();




            CreateMap<UserDto, BeneficiaryToTransactionDto>()
                .ForMember(s => s.Gmail, opt => opt.MapFrom(src => src.Email))
                .ForMember(s => s.Id, opt => opt.Ignore())
                .ForMember(s => s.Cuenta, opt => opt.Ignore())
                .ReverseMap();




        }






    }
}
