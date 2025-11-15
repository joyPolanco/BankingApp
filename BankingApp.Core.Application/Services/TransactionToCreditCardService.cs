

using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;

namespace BankingApp.Core.Application.Services
{
    public class TransactionToCreditCardService : GenericService<Transaction, CreateTransactionDto>, ITransactionToCreditCardService
    {
        private readonly ICreditCardRepository _creditCardRepository;
        private readonly ITransacctionRepository repo;
        private readonly ITransactionService ServiceTransaction;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork unitOfWork;



        public TransactionToCreditCardService(IMapper mapper, ICreditCardRepository creditCardRepository, IUnitOfWork unitOfWork, ITransactionService serviceTransaction, ITransacctionRepository repo) : base(repo, mapper)
        {
            _creditCardRepository = creditCardRepository;
            this.unitOfWork = unitOfWork;
            ServiceTransaction = serviceTransaction;
            this._mapper = mapper;
            this.repo = repo;

        }




        public override async Task<CreateTransactionDto?> AddAsync(CreateTransactionDto Dto)
        {

            await unitOfWork.BeginTransactionAsync();
            try
            {

                var _validateAmount = await ServiceTransaction.ValidateAmount(Dto.Origin, Dto.Amount);
                if (_validateAmount == null || _validateAmount!.IsSuccess == false)
                {
                    return null;
                }


                var _creditCard = await _creditCardRepository.GetByNumberAsync(Dto.Beneficiary);
                decimal descontar = 0;

                if (Dto.Amount > _creditCard!.TotalAmountOwed)
                {

                    descontar = _creditCard.TotalAmountOwed;

                }
                else
                {
                    descontar = Dto.Amount; 
                }


                    var entity = _mapper.Map<Transaction>(Dto);
                if (entity is not null)
                {
                    entity.Amount = descontar;
                    var transac = await repo.AddAsync(entity);
                    var dto = _mapper.Map<CreateTransactionDto>(transac);
                    await unitOfWork.CommitAsync();
                    return dto;
                }

                return null;

            }
            catch
            {

                await unitOfWork.RollbackAsync();
                return default;

            }
        }




        public async Task<CreditCardDto?> DebitTotalAmountOwedAsync(string number,decimal amount)
        {


            try
            {
                var entity = await _creditCardRepository.DebitTotalAmountOwedAsync(number,amount);

                if (entity == null)
                {
                    return null;  
                }

                var map = _mapper.Map<CreditCardDto>(entity);
                return map;

            }
            catch(Exception ex)
            {


                return null;
          
            }
          
        }






        public async Task<List<string>> GetCreditCardByIdUser(string idUsuario)
        {

            try
            {
                List<string> CreditCards = new List<string>();
                var entities = await _creditCardRepository.GetActiveByClientIdAsync(idUsuario);

                if (entities is null)
                {

                    return new List<string>();

                }

                foreach (var entity in entities)
                {

                    CreditCards.Add(entity.Number);
                }


                return CreditCards;

            }
            catch (Exception ex)
            {

                return new List<string>();

            }

        }
    }
}
