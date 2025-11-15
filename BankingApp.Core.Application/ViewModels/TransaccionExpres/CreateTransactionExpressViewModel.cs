
using System.ComponentModel.DataAnnotations;

namespace BankingApp.Core.Application.ViewModels.TransaccionExpres
{
    public class CreateTransactionExpressViewModel
    {


        [Range(0.001, double.MaxValue, ErrorMessage = "El valor del monto debe ser valido y mayor a 0")]
        [Required(ErrorMessage = "Debes ingresar el valor del monto")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Debes ingresar el numero de cuenta origen ")]
        public required string Origin { get; set; }

        [Required(ErrorMessage = "Debes ingresar un numero de cuenta destino ")]
        public required string Beneficiary { get; set; }

    }
}
