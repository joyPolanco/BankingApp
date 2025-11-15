using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class EditUserWithAmountDto
    {
        public string? Id { get; set; }


        public required string Password { get; set; }

        public required string Name { get; set; }

        public string? PhoneNumber { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }

        public decimal? AditionalAmount { get; set; } 


        public bool HasError { get; set; }
        public string? Error { get; set; }
    }
}
