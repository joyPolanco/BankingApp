

using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.X509Certificates;

namespace BankingApp.Core.Application.ViewModels.TransactionToLoan
{
    public class CreateTransactionToLoanViewModal
    {


        public Guid idLoan {  get; set; }
        [Required(ErrorMessage = "Debes indicar el prestamo a pagar")]
        public required string? PublicId { get; set; }
        [Required(ErrorMessage = "Debes indicar la cuenta origen desde cual se decea  pagar")]
        public required string? Cuenta { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "El valor del monto debe ser valido y mayor a 0")]
        [Required(ErrorMessage = "Debes ingresar el valor del monto")]
        public required decimal Amount { get; set; }

    }
}
