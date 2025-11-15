using AutoMapper;
using BankingApp.Core.Application.Dtos.Stats;
using BankingApp.Core.Application.ViewModels.Dashboards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Mappings.DtosAndViewModels
{
    public class AdminDashboardDtoAndViewModelProfile : Profile
    {
        public AdminDashboardDtoAndViewModelProfile()
        {
            CreateMap<AdminDashboardStatsDto, AdminDashboardViewModel>();

        }
    }
}
