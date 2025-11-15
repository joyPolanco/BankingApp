

using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Beneficiary;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Repositories;



namespace BankingApp.Core.Application.Services
{
    public class TransactionExpressService : GenericService<Transaction, CreateTransactionDto>, ITransactionService, ITransactionToBeneficiaryService
    {

        public readonly IMapper _mapper;
        private readonly ITransacctionRepository transacctionRepository;
        private readonly IAccountRepository accountRepository;
        private readonly IBeneficiaryService beneficiaryService;
        private readonly IAccountServiceForWebAPP serviceForWebAPP;
        private readonly IBeneficiaryRepository beneficiaryRepository;
        private readonly IUnitOfWork unitOfWork;


        public TransactionExpressService(IGenericRepository<Transaction> repo,
            IMapper mapper,
            ITransacctionRepository transacctionRepository,
            IAccountRepository accountRepository,
            IBeneficiaryService beneficiaryService,
            IAccountServiceForWebAPP serviceForWebAPP,
            IUnitOfWork unitOfWork,
            IBeneficiaryRepository beneficiaryRepository) : base(repo, mapper)
        {


            this._mapper = mapper;
            this.accountRepository = accountRepository;
            this.transacctionRepository = transacctionRepository;
            this.beneficiaryService = beneficiaryService;
            this.serviceForWebAPP = serviceForWebAPP;
            this.unitOfWork = unitOfWork;
            this.beneficiaryRepository = beneficiaryRepository;
        }





        public override async Task<CreateTransactionDto?> AddAsync(CreateTransactionDto Dto)
        {

            await unitOfWork.BeginTransactionAsync();
            try
            {

                var _validateAccountBeneficiary = await ValidateNumberAsync(Dto.Beneficiary);
                if (_validateAccountBeneficiary == null)
                {

                    return null;

                }


                var _validateAmount = await ValidateAmount(Dto.Origin, Dto.Amount);
                if (_validateAmount == null || _validateAmount!.IsSuccess == false)
                {

                    return null;

                }



                var entity = _mapper.Map<Transaction>(Dto);
                if (entity is not null)
                {
                    var transac = await transacctionRepository.AddAsync(entity);
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





        public async Task<bool> ApproveTransactionAsync(int id, CreateTransactionDto dto)
        {


            try
            {

                if (dto == null || id < 0)
                {

                    return false;

                }


                var _DTO = _mapper.Map<Transaction>(dto);
                bool aproved = await transacctionRepository.ApproveTransaction(id, _DTO);
                if (aproved)
                {


                    return true;


                }


                return false;

            }
            catch (Exception ex)
            {


                return false;


            }
        }




        public async Task<AccountDto?> CreditBalanceAsync(string number, decimal Amount)
        {

            try
            {

                if (number == null || Amount < 0)
                {

                    return null;

                }


                var aproved = await accountRepository.CreditBalance(number, Amount);
                var _DTO = _mapper.Map<AccountDto>(aproved);

                if (_DTO == null)
                {

                    return null;


                }


                return _DTO;

            }
            catch (Exception ex)
            {


                return null;


            }
        }






        public async Task<List<string>?> CuentaListAsync(string idUser)
        {

            try
            {
                var NumeberAccounts = new List<string>();


                if (idUser == null)
                {

                    return new List<string>();

                }



                var Accounts = await accountRepository.GetAllListByIdClienteAsync(idUser);
                if (Accounts != null && Accounts.Any())
                {

                    foreach (var account in Accounts)
                    {
                        NumeberAccounts.Add(account.Number);

                    }
                }



                return NumeberAccounts;



            }
            catch (Exception ex)
            {

                return new List<string>();

            }
        }





        public async Task<AccountDto?> DebitBalanceAsync(string number, decimal Amount)
        {


            try
            {

                if (number == null || Amount < 0)
                {

                    return null;

                }

                var aproved = await accountRepository.DebitBalance(number, Amount);
                var _DTO = _mapper.Map<AccountDto>(aproved);

                if (_DTO == null)
                {

                    return null;

                }



                return _DTO;

            }
            catch (Exception ex)
            {



                return null;


            }




        }







        public async Task<bool> DeclieneTransactionAsync(int id, CreateTransactionDto dto)
        {

            try
            {

                if (dto == null || id < 0)
                {

                    return false;

                }


                var _DTO = _mapper.Map<Transaction>(dto);
                bool aproved = await transacctionRepository.DeclieneTransaction(id, _DTO);
                if (aproved)
                {


                    return true;

                }


                return false;

            }
            catch (Exception ex)
            {


                return false;


            }
        }



        public async Task<List<BeneficiaryToTransactionDto>> GetLisBeneficiary(string IdCliente)
        {
            try
            {


                var ListDataBeneificary = new List<BeneficiaryToTransactionDto>();
                var beneficiaryList = await beneficiaryRepository.GetBeneficiariesByIdCliente(IdCliente);

                foreach (var item in beneficiaryList)
                {

                  var accountBeneficiary =  await  accountRepository.GetAccounByIdClienteAsync(item.BeneficiaryId);
                  var UserBeneficiary =   await  serviceForWebAPP.GetUserById(item.BeneficiaryId);


                    var entity = _mapper.Map<BeneficiaryToTransactionDto>(UserBeneficiary);
                    entity.Id = item.BeneficiaryId;
                    entity.Cuenta = accountBeneficiary!.Number;
                  
                    ListDataBeneificary.Add(entity);
                }

               
                return ListDataBeneificary; 


            }
            catch (Exception ex)
            {

                return new List<BeneficiaryToTransactionDto>();
               
            }

        }




        public async Task<ValidateAmountResponseDto?> ValidateAmount(string number, decimal Amount)
        {
            var response = new ValidateAmountResponseDto() { IsSuccess = true };

            try
            {
                if (number == null || Amount < 0)
                {

                    response.IsSuccess = false;
                    response.Error = "Los valores ingresado no son validos, favor ingresar valores reales";

                }


                var cuenta = await accountRepository.GetAccountByNumber(number!);
                if (cuenta == null)
                {

                    response.IsSuccess = false;
                    response.Error = "No se encontro una cuenta con ese numero!";
                }


                if (cuenta!.Balance < Amount)
                {

                    response.IsSuccess = false;
                    response.Error = "El monto ingresado excede el saldo disponible, Favor verificar";

                }

                response.AccounId = cuenta.Id;
                return response;

            }
            catch (Exception ex)
            {

                return null;

            }

        }









        public async Task<ValidateAccountNumberResponseDto?> ValidateNumberAsync(string number)
        {
            try
            {


                if (number == null)
                {

                    return null;

                }

                var data = await beneficiaryService.ValidateAccountNumberExist(number);
                var entity = await accountRepository.GetAccountByNumber(number);

                if (data != null && entity != null)
                {

                    var map = _mapper.Map<ValidateAccountNumberResponseDto>(data);
                    var beneficiary = await serviceForWebAPP.GetUserById(data.IdBeneficiary);
                    map.AccountBenefiicaryId = entity!.Id;
                    map.LastName = beneficiary!.LastName;
                    map.Gmail = beneficiary.Email;

                    return map;
                }


                return null;

            }
            catch (Exception ex)
            {

                return null;

            }








        }
    }
}
