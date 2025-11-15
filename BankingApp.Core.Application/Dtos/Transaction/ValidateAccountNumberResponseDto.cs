using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Transaction
{
    public class ValidateAccountNumberResponseDto
    {

        public string? BeneficiaryId { get; set; }
        public int AccountBenefiicaryId { get; set; }
        public bool IsExist { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? Gmail { get; set; }

    }
}
