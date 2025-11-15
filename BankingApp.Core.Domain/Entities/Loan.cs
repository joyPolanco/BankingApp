using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Entities
{
    public class Loan
    {

        public Guid Id { get; set; }

        public required string ClientId { get; set; }
        public required string PublicId { get; set; }

        //Total del prestamo
        public decimal TotalLoanAmount { get; set; }


        //Monto restante a pagar  prestamo
        public required decimal OutstandingBalance { get; set; }

        //Tasa aplicada
        public required  decimal InterestRate { get; set; }
        //Plazo del prestamo en meses
        public required int LoanTermInMonths { get; set; }

        public  required LoanStatus Status { get; set; }


        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }

       public ICollection<Installment> ?Installments { get; set; }
        public DateTime UpdatedAt { get; set; }

    }
}
