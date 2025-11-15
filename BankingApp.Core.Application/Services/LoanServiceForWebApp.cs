using AutoMapper;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Entities;
using BankingApp.Core.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Services
{

    //Revisar para el web app
    public class LoanServiceForWebApp : BaseLoanService, ILoanServiceForWebApp
    {
        private readonly IUserService _userService;
        private readonly ILoanRepository _repo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IInstallmentRepository _installmentRepository;
        private readonly IAccountRepository _accountRepository;
        private ILogger<Loan> _logger;
        public LoanServiceForWebApp(
            ILoanRepository repo,
            IMapper mapper,
            ILogger<Loan> logger,
            IUserService userService,
            IUnitOfWork unitOfWork, 
            IInstallmentRepository installmentRepository,
            IAccountRepository accountRepository,
            IEmailService emailService,
            ICreditCardRepository creditCardRepository
            ) 
            : base(repo, mapper, logger, unitOfWork, installmentRepository, accountRepository, emailService, userService, creditCardRepository)
        {
            _repo = repo;
            _userService= userService;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _installmentRepository = installmentRepository;
            _accountRepository = accountRepository;




        }
              public async Task<List<UserDto>> GetClientsAvailableForLoan(string? DocumentId = null)
        {
            var clientDebts = await _repo.GetAllQuery()
                .Where(r => r.IsActive)
                .GroupBy(r => r.ClientId)
                .Select(g => new { ClientId = g.Key, TotalDebt = g.Sum(r => r.OutstandingBalance) })
                .ToDictionaryAsync(x => x.ClientId, x => x.TotalDebt);

            var clientsWithDebt = await _userService.GetClientsWithDebtInfo(clientDebts, DocumentId);

            return clientsWithDebt;
        }


        public async Task<CreateLoanResult> HandleCreateRequest(LoanRequest request)
        {
            var result = new CreateLoanResult();

            var userDebt = await _repo
                .GetAllQuery()
                .Where(r => r.IsActive && r.ClientId == request.ClientId)
                .Select(r => r.OutstandingBalance)
                .SumAsync();

            var systemDebt = await GetTotalLoanDebt();

            result.ClientIsHighRisk = userDebt > systemDebt ||
                (userDebt + ((request.LoanAmount * request.AnualInterest / 100) * (request.LoanTermInMonths / 12))) > systemDebt;

            if (result.ClientIsHighRisk)
            {
                result.ClientIsAlreadyHighRisk = userDebt > systemDebt;
                return result;
            }

            return await Create(request);
        }

    }
}

