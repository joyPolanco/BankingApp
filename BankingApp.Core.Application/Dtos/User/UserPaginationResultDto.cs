using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class UserPaginationResultDto
    {
        public required List <UserDto> Data { get; set; }

        [JsonProperty("totalUsuarios")]

        public int TotalCount { get; set; }
        [JsonProperty("paginaActual")]

        public int CurrentPage { get; set; }
        [JsonProperty("totalPaginas")]

        public int PagesCount { get; set; }
    }
}
