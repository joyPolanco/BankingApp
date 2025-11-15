using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.ViewModels.Beneficiary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ITransactionToBeneficiaryService
    {


        Task<List<BeneficiaryToTransactionDto>> GetLisBeneficiary(string IdCliente);
   

    }
}
