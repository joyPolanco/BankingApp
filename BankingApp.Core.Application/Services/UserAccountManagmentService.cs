using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{
    public class UserAccountManagmentService :IUserAccountManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAccountServiceForWebAPP _accountUserService;
        private readonly ISavingAccountServiceForApi _SavingAccountService;

        public UserAccountManagmentService(IUnitOfWork unitOfWork, IMapper mapper, IAccountServiceForWebAPP accountUserService, ISavingAccountServiceForApi SavingAccountService)
        {
           _unitOfWork = unitOfWork;
            _mapper = mapper;
            _accountUserService=accountUserService;
            _SavingAccountService = SavingAccountService;
        }
        public async Task<RegisterUserWithAccountResponseDto> CreateUserWithAmount(CreateUserDto request, string AdminId, bool ForApi=false, string? origin=null)
        {
            var response = new RegisterUserWithAccountResponseDto();

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var saveUserDto = _mapper.Map<SaveUserDto>(request);
                var user = await _accountUserService.RegisterUser(saveUserDto, origin, ForApi);

                if (user.HasError)
                {
                    response.IsSuccesful = false;
                    response.UserAlreadyExists = true;
                    response.StatusMessage = user.Message;

                    await _unitOfWork.RollbackAsync();
                    return response;
                }

                var accountDto = new AccountDto
                {
                    UserId = user.Id,
                    Id = 0,
                    Type = AccountType.PRIMARY,
                    Number = await _SavingAccountService.GenerateAccountNumber(),
                    Balance = request.InitialAmount ?? 0,
                    AdminId = AdminId
                };

                await _SavingAccountService.AddAsync(accountDto);
                var reponsemap = _mapper.Map<RegisterUserWithAccountResponseDto>(user);
                response = reponsemap;
                response.EntityId = reponsemap.EntityId;
                response.IsSuccesful = true;
                response.StatusMessage = "Usuario y cuenta creados exitosamente.";

                await _unitOfWork.CommitAsync();
                return response;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();

                response.IsSuccesful = false;
                response.IsInternalError = true;
                response.StatusMessage = $"Error interno: {ex.Message}";
                return response;
            }
        }

        public async Task<RegisterUserWithAccountResponseDto> EditUserAndAmountAsync(UpdateUserDto request, string AdminId, bool ForApi = false, string? origin = null)
        {
            var response = new RegisterUserWithAccountResponseDto();
          
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Editar usuario
                var saveUserDto = _mapper.Map<SaveUserDto>(request);
                 var editDto =await _accountUserService.EditUser(saveUserDto, null, true, ForApi);

                if (editDto.HasError)
                {
                    response.IsSuccesful = false;
                    response.UserAlreadyExists = editDto.HasError;
                    response.StatusMessage = editDto.Message;
                    await _unitOfWork.RollbackAsync();
                    return response;
                }
                // Obtener cuenta y actualizar balance
                var account = await _SavingAccountService.GetAccountByClientId(request.Id);

                if (request.AdditionalBalance > 0)
                {
                    account.Balance += request.AdditionalBalance.Value;
                 await _SavingAccountService.UpdateAsync(account.Id, account);
                }

                await _unitOfWork.CommitAsync();

                response = _mapper.Map<RegisterUserWithAccountResponseDto>(editDto);
                response.IsSuccesful = true;
                return response;

            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                response.IsSuccesful = false;
                response.IsInternalError = true;
                return response;
            }
        }


        public async Task<AccountDto?> GetMainSavingAccount (string clientId)
        {
            var account= await _SavingAccountService.GetAccountByClientId (clientId);
            if (account == null) return null;
            return _mapper.Map<AccountDto>(account);
        }


        public async Task ChangeBalanceForClient(string id, decimal AdditionalBalance)
        {
            var account = await _SavingAccountService.GetAccountByClientId(id);
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                account.Balance += AdditionalBalance;
                await _SavingAccountService.UpdateAsync(account.Id, account);
                await _unitOfWork.CommitAsync();

            }
            catch
            {
                await _unitOfWork.RollbackAsync();
            }
        }
    }
}
