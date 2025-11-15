using AutoMapper;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.TransaccionExpres;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.Json;


namespace BankingApp.Areas.Client.Controllers
{

    [Authorize(Roles = "CLIENT")]
    [Area("Client")]
    public class TransactionToBeneficiaryController : Controller
    {


        private readonly IMapper _mapper;
        private readonly ITransactionService transactionService;
        private readonly ITransactionToBeneficiaryService transactionToBeneficiaryService;
        private readonly IEmailService emailService;
        private readonly UserManager<AppUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;



        public TransactionToBeneficiaryController(IMapper _mapper,
            ITransactionService transactionService,
            UserManager<AppUser> userManager, 
            IEmailService emailService, 
            IWebHostEnvironment webHostEnvironment,
            ITransactionToBeneficiaryService transactionToBeneficiaryService)
        {

            this._mapper = _mapper;
            this.transactionService = transactionService;
            this.userManager = userManager;
            this.emailService = emailService;
            this.webHostEnvironment = webHostEnvironment;
            this.transactionToBeneficiaryService = transactionToBeneficiaryService;
        }






        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await userManager.GetUserAsync(User);


            var vm = new CreateTransactionToBeneficiaryViewModel()
            {
                Amount = 0,
                Beneficiary = "",
                Origen = ""

            };

            ViewBag.BeneficiariesAccount = await transactionToBeneficiaryService.GetLisBeneficiary(user!.Id);
            ViewBag.cuentas = await transactionService.CuentaListAsync(user!.Id);
          


            return View(vm);
        }





        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionToBeneficiaryViewModel vm)
        {

            var user = await userManager.GetUserAsync(User);

            if(vm == null)
            {

                ModelState.AddModelError(string.Empty,"Asegurate de ingresar los valores correctamente, verifique he intente otra vez");
                return View(vm);

            }

            var validateAccount = await transactionService.ValidateNumberAsync(vm.Beneficiary!);
            var validateAmount = await transactionService.ValidateAmount(vm.Origen!, vm.Amount);

            if (validateAmount == null || validateAmount.IsSuccess == false)
            {

                ModelState.AddModelError(string.Empty, $"{validateAmount!.Error}");
                //registrar transaccion en caso de ser rechazada
                var Transaccion = _mapper.Map<CreateTransactionDto>(vm);
                Transaccion.Status = OperationStatus.DECLINED;
                Transaccion.Type = TransactionType.DEBIT;
                Transaccion.AccountId = validateAmount!.AccounId;
                Transaccion.AccountNumber = vm.Origen!;
                Transaccion.DateTime = DateTime.UtcNow;
                Transaccion.Description = DescriptionTransaction.Transaccion_A_Beneficiario;

                var salvar = await transactionService.AddAsync(Transaccion);

            }


            if (!ModelState.IsValid)
            {


                ViewBag.BeneficiariesAccount = await transactionToBeneficiaryService.GetLisBeneficiary(user!.Id);
                ViewBag.cuentas = await transactionService.CuentaListAsync(user!.Id);
                return View(vm);
            }


            TempData["TransactionData"] = JsonSerializer.Serialize(vm);
            var dataBeneficary = _mapper.Map<BeneficiaryToTransactionViewModel>(validateAccount);


            return View("Confirm", dataBeneficary);


        }




        [HttpPost]
        public async Task<IActionResult> Confirm()
        {

            var user = await userManager.GetUserAsync(User);


            if (!TempData.ContainsKey("TransactionData"))
            {
                TempData["Error"] = "No se encontró la información de la transacción.";
                return RedirectToAction("Create");
            }


            var model = JsonSerializer.Deserialize<CreateTransactionToBeneficiaryViewModel>(
                TempData["TransactionData"]!.ToString()!
            );



            if (model == null)
            {
                TempData["Error"] = "No se pudo leer la información de la transacción.";
                return RedirectToAction("Create");
            }



            var validateAmount = await transactionService.ValidateAmount(model.Origen!, model.Amount);
            var beneficiary = await transactionService.ValidateNumberAsync(model.Beneficiary!);



            //registrar transaccion

            var Transaccion = _mapper.Map<CreateTransactionDto>(model);
            Transaccion.Status = OperationStatus.APPROVED;
            Transaccion.Type = TransactionType.DEBIT;
            Transaccion.AccountId = validateAmount!.AccounId;
            Transaccion.AccountNumber = model.Origen!;
            Transaccion.DateTime = DateTime.UtcNow;
            Transaccion.Description = DescriptionTransaction.Transaccion_A_Beneficiario;

            var salvar = await transactionService.AddAsync(Transaccion);



            var debitResult = await transactionService.DebitBalanceAsync(model.Origen!, model.Amount);
            if (debitResult == null)
            {
                TempData["Error"] = "No se pudo debitar la cuenta de origen.";
                return RedirectToAction("Create");
            }



            var creditResult = await transactionService.CreditBalanceAsync(model.Beneficiary!, model.Amount);
            if (creditResult == null)
            {
                TempData["Error"] = "No se pudo acreditar la cuenta destino.";
                return RedirectToAction("Create");
            }


            //registrar transaccion para el beneficiario
            var creditTransaction = _mapper.Map<CreateTransactionDto>(model);
            creditTransaction.Status = OperationStatus.APPROVED;
            creditTransaction.Type = TransactionType.CREDIT;
            creditTransaction.AccountId = beneficiary!.AccountBenefiicaryId;
            creditTransaction.AccountNumber = model.Beneficiary!;
            creditTransaction.DateTime = DateTime.UtcNow;
            creditTransaction.Description = DescriptionTransaction.Transaccion_A_Beneficiario;

            var credit = await transactionService.AddAsync(creditTransaction);



            //correo para cliente origen
            var pathClient = Path.Combine(webHostEnvironment.WebRootPath, "HTML", "notificaciones", "NotificationToBeneficiary.html");
            string template = await System.IO.File.ReadAllTextAsync(pathClient);

            template = template.Replace("{{AMOUNT}}", Transaccion.Amount.ToString("C", new CultureInfo("es-DO")))
                               .Replace("{{DATE}}", Transaccion.DateTime.ToString("dd/MM/yyyy HH:mm"));

            await emailService.SendAsync(new EmailRequestDto()
            {
                To = user!.Email,
                Subject = $"Transacción realizada a la cuenta [****{model.Beneficiary![^4..]}]",
                BodyHtml = template
            });



            //correo para el beneficiario
            var pathBeneficiary = Path.Combine(webHostEnvironment.WebRootPath, "HTML", "notificaciones", "NotificationTransactionExpressBeneficiary.html");
            string _template = await System.IO.File.ReadAllTextAsync(pathBeneficiary);

            _template = _template.Replace("{{AMOUNT}}", Transaccion.Amount.ToString("C", new CultureInfo("es-DO")))
                                 .Replace("{{DATE}}", Transaccion.DateTime.ToString("dd/MM/yyyy HH:mm"));

            await emailService.SendAsync(new EmailRequestDto()
            {
                To = beneficiary.Gmail,
                Subject = $"Transacción enviada desde la cuenta [****{model.Origen![^4..]}]",
                BodyHtml = _template
            });


            return RedirectToRoute(new { area = "Client", controller = "Home", action = "Index" });

        }






    }
}
