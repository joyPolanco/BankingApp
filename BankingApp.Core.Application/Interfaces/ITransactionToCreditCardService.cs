

using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Application.Dtos.Transaction;
using System.Transactions;

namespace BankingApp.Core.Application.Interfaces
{
    public interface ITransactionToCreditCardService : IGenericService<Transaction,CreateTransactionDto>
    {


        Task<List<string>> GetCreditCardByIdUser(string idUsuario);
        Task<CreditCardDto?> DebitTotalAmountOwedAsync(string number, decimal amount);


    }
}
