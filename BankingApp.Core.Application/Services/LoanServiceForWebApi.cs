using AutoMapper;
using AutoMapper.QueryableExtensions;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Installment;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;

namespace BankingApp.Core.Application.Services
{
    public class LoanServiceForWebApi : BaseLoanService, ILoanServiceForWebApi
    {
        private readonly ILoanRepository _loanRepository;
        private readonly IMapper _mapper;
        private readonly IInstallmentRepository _installmentRepo;
        private readonly IAccountRepository _accountRepo;
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<Loan> _logger;
        private readonly ICreditCardRepository _cardRepository;

        public LoanServiceForWebApi(
            ILoanRepository repo,
            IMapper mapper,
            IInstallmentRepository installmentRepository,
            ILogger<Loan> logger,
            IAccountRepository accountRepository,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IUserService userService,
            ICreditCardRepository cardRepository
            )
            : base(repo, mapper, logger, unitOfWork, installmentRepository, accountRepository, emailService, userService, cardRepository)
        {
            _loanRepository = repo;
            _mapper = mapper;
            _installmentRepo = installmentRepository;
            _accountRepo = accountRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _cardRepository= cardRepository;

        }

        public async Task<CreateLoanResult> HandleCreateRequestApi(LoanRequest request)
        {
            var result = new CreateLoanResult();

            if (request == null ||
               string.IsNullOrEmpty( request.ClientId)||
                request.LoanAmount <= 0 ||
                request.AnualInterest <= 0 ||
                request.LoanTermInMonths <= 0)
            {
                result.HasValidationErrors = true;
                result.ValidationMessage = "Datos incompletos o inválidos.";
                return result;
            }

      
            result.ClientHasActiveLoan = await ClientHasActiveLoan(request.ClientId);
            if (result.ClientHasActiveLoan)
                return result;

          
            var clientLoansDebt = await GetClientLoansDebt(request.ClientId);
            var cardDebt = await _cardRepository.GetClientTotalCreditCardDebt(request.ClientId);

            var userDebt = clientLoansDebt + cardDebt;

            var systemLoansDebt = await GetTotalLoanDebt();
            var systemCardDebt = await _cardRepository.GetTotalClientsCreditCardDebt();

            var systemDebt = systemLoansDebt + systemCardDebt;

            var monthlyRate = request.AnualInterest / 100m;  
            var interests = request.LoanAmount * monthlyRate * request.LoanTermInMonths;

            var newLoanTotal = request.LoanAmount + interests;

            result.ClientIsHighRisk =
                userDebt > systemDebt ||
                (userDebt + newLoanTotal) > systemDebt;

            if (result.ClientIsHighRisk)
                return result;

            var createResult = await Create(request);

            if (createResult.LoanCreated)
                await SendEmail(request, createResult);

            return createResult;
        }




        public async Task<OperationResultDto> UpdateLoanRateAPI(string publicId, decimal newRate)
        {
            var result = new OperationResultDto { IsSuccessful = false };

            var loan = await _loanRepository
                .GetAllQueryWithInclude(new List<string> { "Installments" })
                .FirstOrDefaultAsync(l => l.PublicId == publicId);

            if (loan is null)
            {
                result.StatusMessage = "Préstamo no encontrado.";
                return result;
            }

            if (!loan.IsActive)
            {
                result.StatusMessage = "No se puede actualizar un préstamo inactivo.";
                return result;
            }

            decimal annualInterest = newRate;
            decimal monthlyRate = (annualInterest / 100m) / 12m;
            decimal n = loan.LoanTermInMonths;
            decimal P = loan.TotalLoanAmount;

            decimal onePlusRPowN = DecimalPow(1 + monthlyRate, n);
            decimal newInstallmentValue = P * (monthlyRate * onePlusRPowN) / (onePlusRPowN - 1);
            newInstallmentValue = Math.Round(newInstallmentValue, 2, MidpointRounding.ToEven);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                loan.InterestRate = newRate;
                loan.UpdatedAt = DateTime.Now;

                foreach (var installment in loan.Installments.Where(i => !i.IsPaid))
                {
                    installment.Value = newInstallmentValue;
                    installment.IsModified = true;
                }

                await _loanRepository.UpdateByObjectAsync(loan);
                await _installmentRepo.UpdateRangeAsync(loan.Installments.ToList());

                await _unitOfWork.CommitAsync();

                result.IsSuccessful = true;
                result.StatusMessage = "Tasa actualizada correctamente.";
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al actualizar la tasa del préstamo {PublicId}", publicId);
                result.StatusMessage = "Ocurrió un error al actualizar la tasa.";
            }

            return result;
        }
        private static decimal DecimalPow(decimal baseValue, decimal exponent)
        {
        
            double result = Math.Pow((double)baseValue, (double)exponent);
            return (decimal)result;
        }





      

       

    }
}
