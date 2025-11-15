using AutoMapper;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.ViewModelsAndDtos
{
    public class UserViewModelMappingProfile: Profile
    {
        public UserViewModelMappingProfile()
        {
            CreateMap<CreateUserViewModel, CreateUserDto>()

                .ForMember(dest => dest.InitialAmount, opt => opt.MapFrom(r => r.InitialAmount));
            CreateMap<EditUserWithAmountViewModel, UpdateUserDto>()
               .ForMember(dest => dest.AdditionalBalance, opt => opt.MapFrom(r => r.AditionalAmount));
            CreateMap<CreateUserViewModel, SaveUserDto>();

        }
    }
}
