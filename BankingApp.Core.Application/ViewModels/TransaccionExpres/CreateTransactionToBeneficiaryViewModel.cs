

using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.TransaccionExpres
{
    public class CreateTransactionToBeneficiaryViewModel
    {

        [Required(ErrorMessage = "Debes seleccionar un beneficiario")]
        public string? Beneficiary { get; set; }
        [Required(ErrorMessage = "Debes Seleccionar una cuenta origen")]
        public string? Origen { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "El valor del monto debe ser valido y mayor a 0")]
        [Required(ErrorMessage = "Debes ingresar el valor del monto")]
        public decimal Amount { get; set; }

    }
}
