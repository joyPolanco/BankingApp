using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UpdateUserDto
    {
        [JsonProperty("usuario")]
        public required string UserName { get; set; }


        [JsonIgnore]
        public  string? Id { get; set; }

        [JsonProperty("correo")]
        public required string Email { get; set; }

        [JsonProperty("contrasena")]
        public string? Password { get; set; }

        [JsonProperty("ConfirmarContrasena")]
        public string? ConfirmPassword { get; set; }


        [JsonProperty("nombre")]
        public required string Name { get; set; }

        [JsonProperty("apellido")]
        public required string LastName { get; set; }

        [JsonProperty("cedula")]
        public required string DocumentIdNumber { get; set; }
        [JsonProperty("montoAdicional")]

        public required decimal ? AdditionalBalance { get; set; }

    }
}
