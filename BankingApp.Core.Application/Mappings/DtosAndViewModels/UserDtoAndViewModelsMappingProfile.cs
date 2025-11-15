using AutoMapper;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.DtosAndViewModels
{
    public class UserDtoAndViewModelsMappingProfile :Profile
    {
        public UserDtoAndViewModelsMappingProfile()
        {
            CreateMap<UserDto, UserViewModel>();

            CreateMap<UserDto, EditUserWithAmountViewModel>()

                .ForMember(dest => dest.Password, opt => opt.MapFrom(src => ""))

                .ForMember(dest => dest.ConfirmPassword, opt => opt.MapFrom(src => ""));
        }
    }
}
