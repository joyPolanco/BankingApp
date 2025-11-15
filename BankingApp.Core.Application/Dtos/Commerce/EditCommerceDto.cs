using Newtonsoft.Json;

namespace BankingApp.Core.Application.Dtos.Commerce
{
    public class EditCommerceDto

    {
        [JsonIgnore]
        public  required int Id { get; set; }
        [JsonProperty("Nombre")]

        public required string Name { get; set; }
        [JsonProperty("Descripcion")]

        public required string Description { get; set; }

        public required string Logo { get; set; }
    }
}
