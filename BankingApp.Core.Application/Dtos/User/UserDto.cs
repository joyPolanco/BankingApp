
using BankingApp.Core.Application.Dtos.Account;
using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UserDto
    {
        public required string Id { get; set; }
        [JsonProperty("usuario")]

        public required string UserName { get; set; }
        [JsonProperty("correo")]

        public required string Email { get; set; }
        [JsonIgnore]
        public bool IsVerified { get; set; }
        [JsonProperty("nombre")]

        public required string Name { get; set; }
        [JsonProperty("apellido")]

        public required string LastName { get; set; }
        [JsonProperty("cedula")]

        public required string DocumentIdNumber { get; set; }
        [JsonProperty("rol")]

        public required string Role { get; set; }

        public bool IsActive { get; set; }
        [JsonProperty("estado")]

        public required string Status { get; set; }


        [JsonIgnore]
        public decimal? TotalDebt { get; set; }


        [JsonProperty("cuentaPrincipal", NullValueHandling = NullValueHandling.Ignore)]
        public PrimaryAccountDto ?MainAccount { get; set; }
    }
}
