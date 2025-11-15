using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.Account
{
    public class CreateSavingsAccountDto
    {
        [Required(ErrorMessage = "La cédula del cliente es requerida")]
        [JsonProperty("cedulaCliente")]
        public required string ClientDocument { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El balance inicial debe ser mayor o igual a 0")]
        [JsonProperty("balanceInicial")]
        public decimal InitialBalance { get; set; } = 0;
    }
}
