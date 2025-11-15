using BankingApp.Core.Application.Dtos.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface IDashboardsStatsService
    {
        Task<AdminDashboardStatsDto> GetAdminStats();
    }
}
