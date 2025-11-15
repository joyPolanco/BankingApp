using BankingApp.Core.Application.Dtos.Account;
using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UserDetailDto
    {
        [JsonProperty("usuario")]
        public required string UserName { get; set; }

        [JsonProperty("nombre")]
        public required string Name { get; set; }

        [JsonProperty("apellido")]
        public required string LastName { get; set; }

        [JsonProperty("cedula")]
        public required string DocumentIdNumber { get; set; }

        [JsonProperty("correo")]
        public required string Email { get; set; }

        [JsonProperty("rol")]
        public required string Role { get; set; }

        [JsonProperty("estado")]
        public required string Status { get; set; }

        [JsonProperty("cuentaPrincipal")]
        public PrimaryAccountDto? PrimaryAccount { get; set; }
    }

 
}
