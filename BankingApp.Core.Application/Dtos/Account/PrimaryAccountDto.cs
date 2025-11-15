using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.Account
{public class PrimaryAccountDto
    {

        [JsonProperty("numeroCuenta")]
        public string? Number { get; set; }

        [JsonProperty("balance")]
        public decimal? Balance { get; set; }
    }
}
