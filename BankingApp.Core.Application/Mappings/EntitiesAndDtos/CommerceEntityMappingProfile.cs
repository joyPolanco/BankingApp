using AutoMapper;
using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Domain.Entities;


namespace BankingApp.Core.Application.Mappings.EntitiesAndDtos
{
    public class CommerceEntityMappingProfile : Profile
    {
        public CommerceEntityMappingProfile()
        {
            CreateMap<Commerce, CommerceDto>()
    .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap< CommerceDto, Commerce>()

            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<CreateCommerceDto, CommerceDto>()
                .ForMember(x => x.Id, opt => opt.MapFrom(src => 0))
                .ForMember(x => x.IsActive, opt => opt.MapFrom(src => true))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<EditCommerceDto, CommerceDto>();


        }
    }
}
