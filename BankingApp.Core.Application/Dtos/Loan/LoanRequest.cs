using Newtonsoft.Json;


namespace BankingApp.Core.Application.Dtos.Loan
{
    public class LoanRequest
    {
        [JsonProperty("ClienteId")]
        public  required string ClientId { get; set; }

        [JsonProperty("InteresAnual")]

        public required decimal AnualInterest { get; set; }
        [JsonProperty("monto")]

        public required decimal LoanAmount { get; set; }
        [JsonProperty("PlazoMeses")]

        public required int LoanTermInMonths { get; set; }

    }
}
