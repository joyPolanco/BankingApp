

using System.Text.Json.Serialization;

namespace BankingApp.Core.Application.ViewModels.TransaccionExpres
{
    public class BeneficiaryToTransactionViewModel
    {


        public string? BeneficiaryId { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }

    }
}
