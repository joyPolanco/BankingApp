using AutoMapper;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.TransactionToCreditCard;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Core.Domain.Entities;
using BankingApp.Infraestructure.Identity.Entities;
using BankingApp.Infraestructure.Shared.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Threading.Tasks;

namespace BankingApp.Areas.Client.Controllers
{

    [Authorize(Roles = "CLIENT")]
    [Area("Client")]
    public class TransactionToCreditCardController : Controller
    {
        private readonly ITransactionService transactionService;
        private readonly ITransactionToCreditCardService transactionToCreditCard;
        private readonly UserManager<AppUser> userManager;
        private readonly IMapper mapper;
        private readonly IEmailService emailService;    
        private readonly IWebHostEnvironment webHostEnvironment;



        public TransactionToCreditCardController(ITransactionService transactionService, ITransactionToCreditCardService transactionToCreditCard, UserManager<AppUser> userManager, IMapper mapper, IWebHostEnvironment webHostEnvironment, IEmailService emailService)
        {
            this.transactionService = transactionService;
            this.transactionToCreditCard = transactionToCreditCard;
            this.userManager = userManager;
            this.mapper = mapper;
            this.webHostEnvironment = webHostEnvironment;
            this.emailService = emailService;
        }





        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await userManager.GetUserAsync(User);
            var entity = new CreateTransactionToCreditCardViewModel()
            {


                Amount = 0,
                CreditCard = "",
                Account = ""

            };

            ViewBag.CuentasAhorros = await transactionService.CuentaListAsync(user!.Id);
            ViewBag.CreditCars = await transactionToCreditCard.GetCreditCardByIdUser(user!.Id);

            return View(entity);
        }



        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionToCreditCardViewModel vm)
        {
            var user = await userManager.GetUserAsync(User);


            var validateAmount = await transactionService.ValidateAmount(vm.Account,vm.Amount);
            if (validateAmount != null && validateAmount.IsSuccess == false)
            {
                ModelState.AddModelError(string.Empty,$"{validateAmount.Error}");
                //registrar transaccion en caso de ser rechazada
                var Transaccion = mapper.Map<CreateTransactionDto>(vm);
                Transaccion.Status = OperationStatus.DECLINED;
                Transaccion.Type = TransactionType.DEBIT;
                Transaccion.AccountId = validateAmount!.AccounId;
                Transaccion.AccountNumber = vm.Account;
                Transaccion.DateTime = DateTime.UtcNow;
                Transaccion.Description = DescriptionTransaction.Trasaccion_A_Tarjeta;

                var salvar = await transactionService.AddAsync(Transaccion);
            }



            if (!ModelState.IsValid)
            {
                ViewBag.CuentasAhorros = await transactionService.CuentaListAsync(user!.Id);
                ViewBag.CreditCars = await transactionToCreditCard.GetCreditCardByIdUser(user!.Id);
                return View(vm);
            }



            var TransaccionApprove = mapper.Map<CreateTransactionDto>(vm);
            TransaccionApprove.Status = OperationStatus.APPROVED;
            TransaccionApprove.Type = TransactionType.CREDIT;
            TransaccionApprove.AccountId = validateAmount!.AccounId;
            TransaccionApprove.AccountNumber = vm.Account;
            TransaccionApprove.DateTime = DateTime.UtcNow;
            TransaccionApprove.Description = DescriptionTransaction.Trasaccion_A_Tarjeta;

            var gualdar = await transactionToCreditCard.AddAsync(TransaccionApprove);
            if (gualdar == null)
            {
                ViewBag.CuentasAhorros = await transactionService.CuentaListAsync(user!.Id);
                ViewBag.CreditCars = await transactionToCreditCard.GetCreditCardByIdUser(user!.Id);
                ModelState.AddModelError(string.Empty,"No se pudo realizan la transaccion");
                return View(vm);
            
            }


            //debitar balance de la cuenta de ahorro
            await transactionService.DebitBalanceAsync(vm.Account, gualdar!.Amount);
            await transactionToCreditCard.DebitTotalAmountOwedAsync(vm.CreditCard, gualdar.Amount);


            var pathBeneficiary = Path.Combine(webHostEnvironment.WebRootPath, "HTML", "notificaciones", "NotificationToCreditCard.html");
            string _template = await System.IO.File.ReadAllTextAsync(pathBeneficiary);

            _template = _template.Replace("{{AMOUNT}}", gualdar.Amount.ToString("C", new CultureInfo("es-DO")))
                                 .Replace("{{Cuenta}}", $"[*****{vm.Account[^4..]}]")
                                 .Replace("{{DATE}}", gualdar.DateTime.ToString("dd/MM/yyyy HH:mm"));

            await emailService.SendAsync(new EmailRequestDto()
            {
                To = user!.Email,
                Subject = $"Pago realizado a la tarjeta [****{gualdar.Beneficiary[^4..]}]",
                BodyHtml = _template
            });


            return RedirectToRoute(new { area = "Client", controller = "Home", action = "Index" });
        }
    }
}
