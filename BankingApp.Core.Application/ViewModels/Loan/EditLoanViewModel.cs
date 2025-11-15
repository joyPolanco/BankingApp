using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.ViewModels.Loan
{
    public class EditLoanViewModel
    {
        public required string PublicId { get; set; }

        [Required(ErrorMessage ="La tasa es requerida")]
        public  decimal Rate { get; set; }
    }
}
