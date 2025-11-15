



using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.User
{
    public class CreateUserDto
    {
        [JsonProperty("usuario")]
        public required string UserName { get; set; }

        [JsonProperty("correo")]

        public required string Email { get; set; }

        [JsonProperty("contrasena")]

        public required string Password { get; set; }
        [JsonProperty("ConfirmContrasena")]

        public required string ConfirmPassword { get; set; }
        [JsonProperty("rol")]

        public required string Role { get; set; }

        [JsonProperty("nombre")]

        public required string Name { get; set; }
        [JsonProperty("apellido")]

        public required string LastName { get; set; }
        [JsonProperty("cedula")]

        public required string DocumentIdNumber { get; set; }
        [JsonProperty("montoInicial")]

        public decimal? InitialAmount { get; set; } // Monto inicial para clientes
        [JsonIgnore]
        public List<string> ?Roles { get; set; }
    }
}
