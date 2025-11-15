using Newtonsoft.Json;
namespace BankingApp.Core.Application.Dtos.Loan
{
    public class LoanPaginationResultDto
    {
        public required List<LoanDto> Data { get; set; }


        [JsonProperty("paginaActual")]

        public int CurrentPage { get; set; }
        [JsonProperty("totalPaginas")]

        public int PagesCount { get; set; }
    }
}
