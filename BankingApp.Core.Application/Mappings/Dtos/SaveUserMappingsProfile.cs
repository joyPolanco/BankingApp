using AutoMapper;
using BankingApp.Core.Application.Dtos.User;


namespace BankingApp.Core.Application.Mappings.Dtos
{
    public class SaveUserMappingsProfile :Profile
    {
        public SaveUserMappingsProfile()
        {
            CreateMap<CreateUserDto, SaveUserDto>()
                .ReverseMap();

            CreateMap<UpdateUserDto, SaveUserDto>()
                .ForMember(dest=>dest.Password, opt=>opt.MapFrom(dest=>dest.Password));
            CreateMap<RegisterUserResponseDto, RegisterUserWithAccountResponseDto>()
                .ForMember(r => r.IsSuccesful, opt => opt.Ignore())
              .ForMember(r => r.EntityId, opt => opt.MapFrom(src=>src.Id))

                .ForMember(r => r.UserAlreadyHasAccount, opt => opt.Ignore());
            CreateMap<EditUserDto, RegisterUserWithAccountResponseDto>()
               .ForMember(r => r.IsSuccesful, opt => opt.Ignore())
               .ForMember(r => r.UserAlreadyHasAccount, opt => opt.Ignore());

            CreateMap<EditUserWithAmountDto, UpdateUserDto>()
       .ForMember(r => r.AdditionalBalance, opt => opt.MapFrom(src => src.AditionalAmount));
        }
    }
}
