using AutoMapper;
using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.Transaction;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.TransactionToLoan;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;


namespace BankingApp.Areas.Client.Controllers
{


    [Authorize(Roles = "CLIENT")]
    [Area("Client")]
    public class TransactionToLoanController : Controller
    {


        private readonly IMapper _mapper;
        private readonly ITransactionService transactionService;
        private readonly ITransactionToLoanService transactionToLoanService;
        private readonly IEmailService emailService;
        private readonly UserManager<AppUser> userManager;
        private readonly IWebHostEnvironment webHostEnvironment;



        public TransactionToLoanController(IMapper _mapper, 
            ITransactionService transactionService,
            UserManager<AppUser> userManager,
            IEmailService emailService,
            IWebHostEnvironment webHostEnvironment,
            ITransactionToLoanService service)
        {

            this._mapper = _mapper;
            this.transactionService = transactionService;
            this.userManager = userManager;
            this.emailService = emailService;
            this.webHostEnvironment = webHostEnvironment;
            transactionToLoanService = service;
        }






        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await userManager.GetUserAsync(User);

            var vm = new CreateTransactionToLoanViewModal()
            {

                Amount = 0m,
                Cuenta = "",
                PublicId = ""

            };

            ViewBag.Prestamos = await transactionToLoanService.GetLoanActive(user!.Id);
            ViewBag.cuentas =await  transactionService.CuentaListAsync(user!.Id);
            return View(vm);
        }




        [HttpPost]
        public async  Task<IActionResult> Create(CreateTransactionToLoanViewModal vm)
        {


            var user = await userManager.GetUserAsync(User);


            if (!ModelState.IsValid) 
            {

                ViewBag.Prestamos = await transactionToLoanService.GetLoanActive(user!.Id);
                ViewBag.cuentas = await transactionService.CuentaListAsync(user!.Id);
                ModelState.AddModelError(string.Empty,"Debes asegurarte de ingresar todo los datos correctamente, favor verificar");
                return View(vm);         
            
           
            }


            var validateAmount = await transactionService.ValidateAmount(vm.Cuenta!,vm.Amount);
            if (validateAmount != null && validateAmount.IsSuccess == false)
            {
                ModelState.AddModelError(string.Empty,$"{validateAmount.Error}");

                ViewBag.Prestamos = await transactionToLoanService.GetLoanActive(user!.Id);
                ViewBag.cuentas = await transactionService.CuentaListAsync(user!.Id);

                //registrar transaccion en caso de ser rechazada
                var Transaccion = _mapper.Map<CreateTransactionDto>(vm);
                Transaccion.Status = OperationStatus.DECLINED;
                Transaccion.Type = TransactionType.CREDIT;
                Transaccion.AccountId = validateAmount!.AccounId;
                Transaccion.AccountNumber = vm.Cuenta!;
                Transaccion.DateTime = DateTime.UtcNow;
                Transaccion.Description = DescriptionTransaction.Transaccion_A_Prestamo;

                var salvar = await transactionService.AddAsync(Transaccion);
                return View(vm);
            }



            var loan = await transactionToLoanService.GetLoanBypublicIdAsync(vm.PublicId!);
            if(loan == null)
            {


                ModelState.AddModelError(string.Empty,"No se encontro el prestamo a pagar, favor intentelo otra vez");
            
            }
            var response = await transactionToLoanService.PayLoanAsync(loan!.Id,vm.Amount);

            if(response != null && response.HasError == true)
            {

                ModelState.AddModelError(string.Empty,$"{response.Error}");
                
            }


            if (!ModelState.IsValid) 
            {

                ViewBag.Prestamos = await transactionToLoanService.GetLoanActive(user!.Id);
                ViewBag.cuentas = await transactionService.CuentaListAsync(user!.Id);
                return View(vm);
            }


            //registrar transaccion

            var Approve = _mapper.Map<CreateTransactionDto>(vm);
            Approve.Status = OperationStatus.APPROVED;
            Approve.Type = TransactionType.CREDIT;
            Approve.AccountId = validateAmount!.AccounId;
            Approve.AccountNumber = vm.Cuenta!;
            Approve.DateTime = DateTime.UtcNow;
            Approve.Description = DescriptionTransaction.Transaccion_A_Prestamo;

            var guardar = await transactionToLoanService.AddAsync(Approve);


            if (guardar == null)
            {

                ViewBag.Prestamos =await  transactionToLoanService.GetLoanActive(user!.Id);
                ViewBag.cuentas =  await transactionService.CuentaListAsync(user!.Id);
                ModelState.AddModelError(string.Empty,"La transaccion no se pudo realizar correctamente, favor volver a intentar");
                return View(vm);
           
            }


            var pathLoan = Path.Combine(webHostEnvironment.WebRootPath, "HTML", "notificaciones", "NotificationToLoan.html");
            string _template = await System.IO.File.ReadAllTextAsync(pathLoan);

            _template = _template.Replace("{{AMOUNT}}", guardar.Amount.ToString("C", new CultureInfo("es-DO")))
                                 .Replace("{{Cuenta}}", $"[*****{vm.Cuenta![^4..]}]")
                                 .Replace("{{DATE}}", guardar.DateTime.ToString("dd/MM/yyyy HH:mm"));

            await emailService.SendAsync(new EmailRequestDto()
            {
                To = user!.Email,
                Subject = $"Pago realizado al préstamo [{vm.PublicId!}]",
                BodyHtml = _template
            });


            return RedirectToRoute(new { area = "Client", controller = "Home", action = "Index" });

        }
    }
}
