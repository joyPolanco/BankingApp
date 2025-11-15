using BankingApp.Core.Application.ViewModels.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class ClientsPageViewModel
    {
        public decimal ClientsDebt { get; set; }
        public required  List<UserViewModel> Clients {  get; set; }

        public string ?DocumentIdFilter { get; set; }


    }
}
