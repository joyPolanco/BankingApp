using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Installment
{
    public class InstallmentViewModel
    {
       
        public required int Number { get; set; }

        public required DateOnly PayDate { get; set; }

        public required Decimal Value { get; set; }


        public bool IsPaid { get; set; } = false;

        public bool IsDelinquent { get; set; } = false;
    }
}
