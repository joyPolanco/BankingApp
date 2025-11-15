using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Beneficiary
{
    public class BeneficiaryToTransactionDto
    {

        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Gmail { get; set; }
        public string? Cuenta { get; set; }

    }
}
