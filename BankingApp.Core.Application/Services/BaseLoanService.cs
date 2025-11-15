using AutoMapper;
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


namespace BankingApp.Core.Application.Services
{
    public abstract class BaseLoanService : GenericService<Loan, LoanDto>, IBaseLoanService
    {
        private readonly IMapper _mapper;
        private readonly ILogger<Loan> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoanRepository _repo;
        private readonly IInstallmentRepository _installmentRepo;
        private readonly IAccountRepository _accountRepository;
        private readonly IEmailService _emailService;
        private readonly IUserService _UserService;
        private readonly ICreditCardRepository _cardRepository;

        public BaseLoanService(ILoanRepository repo, IMapper mapper, ILogger<Loan> logger, IUnitOfWork unitOfWork, IInstallmentRepository installmentRepository, IAccountRepository accountRepository, IEmailService emailService, IUserService userService, ICreditCardRepository cardRepository) : base(repo, mapper)
{
     
            _repo = repo;
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _installmentRepo = installmentRepository;
            _accountRepository = accountRepository;
            _emailService = emailService;
            _UserService = userService;
            _cardRepository = cardRepository;
        }


        public async Task<string> GenerateLoanId()
        {
            bool LoanIdExists = false;
            string Id;
            do
            {
                Id = new string(
                         Guid.NewGuid()
                         .ToString("N")
                         .Where(char.IsDigit)
                         .Take(9)
                         .ToArray());
                LoanIdExists = await _repo.LoanPublicIdExists(Id);

            } while (LoanIdExists);

            return Id;

        }

        public async Task<LoanPaginationResultDto> GetAllFiltered(int page = 1, int pageSize = 20, string? state = null, string? clientId = null)
        {
            var query = _repo.GetAllQueryWithInclude(new List<string> { "Installments" });

            if (!string.IsNullOrEmpty(state))
            {
                try
                {
                    var statusEnum = EnumMapper<LoanStatus>.FromString(state);
                    query = query.Where(r => r.Status == statusEnum);
                }
                catch
                {

                }
            }

            if (!string.IsNullOrEmpty(clientId))
            {
                query = query.Where(r => r.ClientId == clientId);
            }

            var totalCount = await query.CountAsync();

            query = query
                .Skip(pageSize * (page - 1))
                .Take(pageSize);

            var data = await query.ToListAsync();
            var mapped = _mapper.Map<List<LoanDto>>(data);

            return new LoanPaginationResultDto
            {
                Data = mapped,
                PagesCount = (int)Math.Ceiling((double)totalCount / pageSize),
                CurrentPage = page,
            };
        }


        public async Task<decimal> GetTotalLoanDebt()
        {
            return await _repo.GetAllQuery().Where(r => r.IsActive).SumAsync(r => r.OutstandingBalance);
        }
        public async Task<bool> ClientHasActiveLoan(string clientId)
        {

            return await _repo.GetAllQuery().Where(r => r.IsActive && r.ClientId == clientId).AnyAsync();
        }

     public async Task<decimal> GetClientLoansDebt( string clientId)
        {
            var userDebt = await _repo
              .GetAllQuery()
              .Where(r => r.IsActive && r.ClientId ==clientId)
              .Select(r => r.OutstandingBalance)
              .SumAsync();

            return userDebt;

        }



        public async Task<CreateLoanResult> HandleCreateRequestApi(LoanRequest request)
        {
            var result = new CreateLoanResult();

            result.ClientHasActiveLoan = await ClientHasActiveLoan(request.ClientId);
            if (result.ClientHasActiveLoan)
                return result;

            var userDebt = await _repo
                .GetAllQuery()
                .Where(r => r.IsActive && r.ClientId == request.ClientId)
                .Select(r => r.OutstandingBalance)
                .SumAsync();

            var systemDebt = await GetAverageLoanDebth();

            if (systemDebt > 0)
            {
                result.ClientIsHighRisk = userDebt > systemDebt || (userDebt + request.LoanAmount) > systemDebt;
                if (result.ClientIsHighRisk)
                    return result;
            }


            decimal annualInterest = request.AnualInterest;
            decimal monthlyRate = (annualInterest / 100m) / 12m;
            decimal n = request.LoanTermInMonths;
            decimal P = request.LoanAmount;

            decimal onePlusRPowN = DecimalPow(1 + monthlyRate, n);
            decimal constantPayment = P * (monthlyRate * onePlusRPowN) / (onePlusRPowN - 1);
            constantPayment = Math.Round(constantPayment, 2, MidpointRounding.ToEven);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;

                var loanEntity = new Loan
                {
                    Id = Guid.NewGuid(),
                    ClientId = request.ClientId,
                    LoanTermInMonths = request.LoanTermInMonths,
                    InterestRate = request.AnualInterest,
                    OutstandingBalance = request.LoanAmount,
                    TotalLoanAmount = request.LoanAmount,
                    PublicId = await GenerateLoanId(),
                    Status = LoanStatus.ONTIME,
                    IsActive = true,
                    CreatedAt = now
                };
                await _repo.AddAsync(loanEntity);

                var installments = new List<Installment>(request.LoanTermInMonths);
                var firstDueDate = DateOnly.FromDateTime(now.AddMonths(1));

                decimal remainingBalance = P;

                for (int i = 0; i < request.LoanTermInMonths; i++)
                {
                    var dueDate = firstDueDate.AddMonths(i);
                    decimal interest = Math.Round(remainingBalance * monthlyRate, 2);
                    decimal principal = Math.Round(constantPayment - interest, 2);


                    if (i == request.LoanTermInMonths - 1)
                        principal = remainingBalance;

                    remainingBalance -= principal;

                    installments.Add(new Installment
                    {
                        LoanId = loanEntity.Id,
                        Id = 0,
                        Number = i + 1,
                        Value = constantPayment,
                        PayDate = dueDate,
                        IsPaid = false,
                        IsDelinquent = dueDate < DateOnly.FromDateTime(now)
                    });
                }


                await _installmentRepo.AddRangeAsync(installments);

                var account = await _accountRepository
                    .GetAllQuery()
                    .FirstOrDefaultAsync(a => a.ClientId == request.ClientId && a.Type == AccountType.PRIMARY);

                if (account is not null)
                {
                    account.Balance += P;
                    await _accountRepository.UpdateAsync(account.Id, account);
                }

                await _unitOfWork.CommitAsync();
                result.LoanCreated = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al crear el préstamo para el cliente {ClientId}", request.ClientId);

                result.LoanCreated = false;
            }

            await _repo.GetAllQuery().Where(r => r.IsActive && r.ClientId == request.ClientId).AnyAsync();
            return result;
  
        }




        public async Task VerifyAndMarkDelayedLoansAsync()
        {
            const int batchSize = 200;
            int skip = 0;
            int totalProcessed = 0;


            while (true)
            {
                var loans = await _repo.GetAllQueryWithInclude(new List<string> { "Installments" })
                          .Where(l => l.Installments.Any(i => !i.IsPaid && i.PayDate < DateOnly.FromDateTime(DateTime.Now)))
                          .Skip(skip)
                          .Take(batchSize)
                          .ToListAsync();


                if (!loans.Any()) break;


                foreach (var loan in loans)
                {
                    bool hasDelay = false;

                    foreach (var installment in loan.Installments)
                    {
                        if (!installment.IsPaid && installment.PayDate < DateOnly.FromDateTime(DateTime.Now))
                        {
                            installment.IsDelinquent = true;
                            hasDelay = true;
                        }
                    }

                    if (hasDelay)
                        loan.Status = LoanStatus.DELIQUENT;

                    await _repo.UpdateByObjectAsync(loan);

                    totalProcessed += loans.Count();
                    skip += batchSize;
                    _logger.LogInformation("Procesados {count} préstamos hasta ahora.", totalProcessed);

                }
            }
        }

        public async Task<DetailedLoanDto?> GetDetailed(string Id)
        {
            var loan = await _repo.GetAllQuery().Where(r => r.PublicId == Id).FirstOrDefaultAsync();

            if (loan == null) return null;

            return new DetailedLoanDto
            {
                LoadId = loan.PublicId,
                Installments = _mapper.Map<List<InstallmentDto>>(loan.Installments)
            };
        }




        public static decimal DecimalPow(decimal baseValue, decimal exponent)
        {

            double result = Math.Pow((double)baseValue, (double)exponent);
            return (decimal)result;
        }



        public async Task<OperationResultDto> UpdateLoanRate(string publicId, decimal newRate)
        {
            var result = new OperationResultDto { IsSuccessful = false };

            var loan = await _repo
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

                await _repo.UpdateByObjectAsync(loan);
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
        public async Task<CreateLoanResult> Create(LoanRequest request)
        {
            var result = new CreateLoanResult();

            decimal annualInterest = request.AnualInterest;
            decimal monthlyRate = (annualInterest / 100m) / 12m;
            decimal n = request.LoanTermInMonths;
            decimal P = request.LoanAmount;

            decimal onePlusRPowN = DecimalPow(1 + monthlyRate, n);
            decimal constantPayment = P * (monthlyRate * onePlusRPowN) / (onePlusRPowN - 1);
            constantPayment = Math.Round(constantPayment, 2, MidpointRounding.ToEven);

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;

                var loanEntity = new Loan
                {
                    Id = Guid.NewGuid(),
                    ClientId = request.ClientId,
                    LoanTermInMonths = request.LoanTermInMonths,
                    InterestRate = request.AnualInterest,
                    OutstandingBalance = request.LoanAmount,
                    TotalLoanAmount = request.LoanAmount,
                    PublicId = await GenerateLoanId(),
                    Status = LoanStatus.ONTIME,
                    IsActive = true,
                    CreatedAt = now
                };
                await _repo.AddAsync(loanEntity);

                // Crear cuotas
                var installments = new List<Installment>(request.LoanTermInMonths);
                var firstDueDate = DateOnly.FromDateTime(now.AddMonths(1));

                for (int i = 0; i < request.LoanTermInMonths; i++)
                {
                    var dueDate = firstDueDate.AddMonths(i);
                    installments.Add(new Installment
                    {
                        LoanId = loanEntity.Id,
                        Id = 0,
                        Number = i + 1,
                        Value = constantPayment,
                        PayDate = dueDate,
                        IsPaid = i == 0,
                        IsDelinquent = dueDate < DateOnly.FromDateTime(now)
                    });
                }

                await _installmentRepo.AddRangeAsync(installments);

                var account = await _accountRepository
                    .GetAllQuery()
                    .FirstOrDefaultAsync(a => a.UserId == request.ClientId && a.Type == AccountType.PRIMARY);

                if (account is not null)
                {
                    account.Balance += P;
                    await _accountRepository.UpdateAsync(account.Id, account);
                }

                await _unitOfWork.CommitAsync();
                result.LoanCreated = true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Error al crear el préstamo para el cliente {ClientId}", request.ClientId);
                result.LoanCreated = false;
            }

            return result;
        }

        public async Task<CreateLoanResult> SendEmail(LoanRequest request, CreateLoanResult createLoanResult)
        {
            var user = await _UserService.GetUserById(request.ClientId);

            string emailBody = $@"
        <html>
            <body>
                <p>Estimado/a {user.Name} {user.LastName},</p>
                <p>Nos complace informarle que su solicitud de préstamo ha sido aprobada.</p>
                <table style='border-collapse: collapse;'>
                    <tr>
                        <td><strong>Monto del préstamo:</strong></td>
                        <td>{request.LoanAmount:C}</td>
                    </tr>
                    <tr>
                        <td><strong>Interés anual:</strong></td>
                        <td>{request.AnualInterest}%</td>
                    </tr>
                    <tr>
                        <td><strong>Plazo:</strong></td>
                        <td>{request.LoanTermInMonths} meses</td>
                    </tr>
                </table>
                <p>Le agradecemos por confiar en nosotros.</p>
                <p>Atentamente,<br/>Banco XYZ</p>
            </body>
        </html>
    ";

            await _emailService.SendAsync(new EmailRequestDto()
            {
                Subject = "Asignación de Préstamo",
                To = user.Email,
                BodyHtml = emailBody
            });

            return createLoanResult;
        }

    }


}
