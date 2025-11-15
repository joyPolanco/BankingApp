using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Domain.Entities;


namespace BankingApp.Core.Application.Interfaces
{
    public interface ITransactionService : IGenericService<Transaction,CreateTransactionDto>
    {
        Task<bool> ApproveTransactionAsync(int id, CreateTransactionDto dto);
        Task<bool> DeclieneTransactionAsync(int id, CreateTransactionDto dto);
    
        Task<ValidateAccountNumberResponseDto?> ValidateNumberAsync(string number);
        Task<AccountDto?> CreditBalanceAsync(string number,decimal Amount);
        Task<AccountDto?> DebitBalanceAsync(string number,decimal Amount);
        Task<ValidateAmountResponseDto?> ValidateAmount(string number,decimal Amount);
        Task<List<string>?> CuentaListAsync(string idUser);


    }
}
