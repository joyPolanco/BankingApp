using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.CreditCard
{
    public class CreateCreditCardDto
    {
        [Required(ErrorMessage = "El ID del cliente es requerido")]
        [JsonProperty("clienteId")]
        public required string ClientId { get; set; }

        [Required(ErrorMessage = "El límite de crédito es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El límite de crédito debe ser mayor a 0")]
        [JsonProperty("limite")]
        public decimal CreditLimitAmount { get; set; }
    }
}
