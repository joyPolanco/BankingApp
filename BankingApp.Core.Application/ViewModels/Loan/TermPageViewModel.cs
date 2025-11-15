using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class TermPageViewModel
    {
        [Required(ErrorMessage ="El plazo del prestamo es requerido")]
        public int TermInMonths { get; set; }
        [Required(ErrorMessage = "El cliente  es requerido")]

        public string ClientId { get; set; }
        [Required(ErrorMessage = "El monto del prestamo es requerido")]

        public decimal Amount { get; set; }
        [Required(ErrorMessage = "La tasa del prestamo es requerida")]

        public decimal AnnualInterestRate { get; set; }

    }
}
