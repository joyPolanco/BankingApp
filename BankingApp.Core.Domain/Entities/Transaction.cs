using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Domain.Entities
{

    // Para las cuentas
    public class Transaction
    {
        public Guid Id { get; set; }
        //Monto
        public decimal Amount { get; set; }

        public DateTime DateTime { get; set; } = DateTime.Now;

        public TransactionType Type { get; set; }

        public required string AccountNumber { get; set; }
    
        public int AccountId { get; set; }

        public Account ?Account { get; set; }

        public required string   Origin { get; set; }
        public required string Beneficiary { get; set; }
        public OperationStatus Status { get; set; }
        public DescriptionTransaction Description { get; set; }
    }
}
