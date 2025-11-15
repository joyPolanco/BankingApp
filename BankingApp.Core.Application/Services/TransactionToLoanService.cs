
using AutoMapper;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;


namespace BankingApp.Core.Application.Services
{
    public class TransactionToLoanService : GenericService<Transaction, CreateTransactionDto>, ITransactionToLoanService
    {

        private readonly IMapper _mapper;
        private readonly ITransactionService service;
        private readonly IUnitOfWork unitOfWork;
        private readonly ITransacctionRepository repo;
        private readonly IAccountRepository accountRepository;
        private readonly ISavingsAccountServiceForWebApp bankAccountService;
        private readonly IInstallmentRepository installmentRepository;
        private readonly ILoanRepository loanRepo;



        public TransactionToLoanService(IMapper _mapper,
            ITransactionService service,
            IUnitOfWork unitOfWork,
            ITransacctionRepository repo,
            ILoanRepository loanRepo,
            IInstallmentRepository installmentRepository,
            IAccountRepository accountRepository,
            ISavingsAccountServiceForWebApp _service)
            : base(repo, _mapper)
        {

            this._mapper = _mapper;
            this.repo = repo;
            this.loanRepo = loanRepo;
            this.service = service;
            this.installmentRepository = installmentRepository;
            this.unitOfWork = unitOfWork;
            this.accountRepository = accountRepository;
            this.bankAccountService = _service;
        }




        public override async Task<CreateTransactionDto?> AddAsync(CreateTransactionDto Dto)
        {

            await unitOfWork.BeginTransactionAsync();
            try
            {


                var _validateAmount = await service.ValidateAmount(Dto.Origin, Dto.Amount);
                if (_validateAmount == null || _validateAmount!.IsSuccess == false)
                {

                    return null;

                }



                var entity = _mapper.Map<Transaction>(Dto);
                if (entity is not null)
                {
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





        public async Task<List<string>> GetLoanActive(string idCliente)
        {
            try
            {


                var entities = await loanRepo.GetLoanListByIdClient(idCliente);
                List<string> Loans = new List<string>();

                if (entities == null)
                {

                    return new List<string>();

                }


                foreach (var entity in entities)
                {

                    Loans.Add(entity.PublicId);

                }


                return Loans;


            }
            catch (Exception ex)
            {

                return new List<string>();

            }
        }

        public async Task<LoanDto?> GetLoanBypublicIdAsync(string publicId)
        {

            try
            {

                var entity = await loanRepo.GetLoanByPublicId(publicId);


                if(entity == null)
                {

                    return null;
                
                }


                var map = _mapper.Map<LoanDto>(entity); 
                return map;
            }
            catch (Exception ex)
            {

                return null;

            }

        }



        public async Task<PayLoanResponseDto?> PayLoanAsync(Guid LoanId, decimal amount)
        {
            await unitOfWork.BeginTransactionAsync();
            var response = new PayLoanResponseDto() { HasError = false };
            try
            {

                var Installment = await installmentRepository.GetListInstallamentByLoanId(LoanId);

                if (Installment == null || !Installment.Any())
                {

                    response.HasError = true;
                    response.Error = "No se encontraron cuotas a pagar para dicho prestamo";
                }

                var debitInstallmentList = Installment!
                    .Where(s => s.IsPaid == false)
                    .OrderBy(s => s.PayDate)
                    .ToList();

                foreach (var intem in debitInstallmentList)
                {

                    if (amount <= 0)
                        break;


                    var entity = intem;


                    decimal descontar = 0m;
                    if (amount > entity!.Value)
                    {

                        descontar = entity.Value;

                    }
                    else
                    {

                        descontar = amount;

                    }
                    amount -= descontar;



                    var Loanclient = await loanRepo.PayLoan(entity.LoanId, descontar, debitInstallmentList.Count);


                    if (entity.Value <= 0)
                    {
                        break;
                    }

                   
                    if (Loanclient is not null)
                    {
                        await installmentRepository.UpdateInstallmentOnPaymentAsync(entity.Id, entity,descontar);
                        var AccountCliet = await bankAccountService.GetAccountByClientId(Loanclient!.ClientId);
                        await accountRepository.DebitBalance(AccountCliet!.Number, descontar);
                        response.HasError = false;
                        response.Error = "";
                        response.IdLoan = entity.LoanId;
                    }

                }

                await unitOfWork.CommitAsync();
                return response;
            }
            catch (Exception ex)
            {
                await unitOfWork.RollbackAsync();
                response.HasError = true;
                response.Error = ex.Message;
                return response;

            }
        }






    }
}
