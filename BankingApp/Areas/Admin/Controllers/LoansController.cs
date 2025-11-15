using AutoMapper;
using BankingApp.Core.Application.Dtos;
using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.Loan;
using BankingApp.Core.Application.ViewModels.User;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace YourNamespace.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class LoansController : Controller
    {
        private readonly ILoanServiceForWebApp _service;
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public LoansController(ILoanServiceForWebApp service, IMapper mapper, IUserService userService)
        {
            _service = service;
            _mapper = mapper;
            _userService = userService;
        }

        // Index: listado con filtros y paginación
        public async Task<IActionResult> Index(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] LoanStatus? status = null,
            [FromQuery] string? documentId = null)
        {
            var user = string.IsNullOrWhiteSpace(documentId)
                ? null
                : await _userService.GetByDocumentId(documentId);

            var statusString = status.HasValue ? EnumMapper<LoanStatus>.ToString(status.Value) : null;

            var pag = await _service.GetAllFiltered(page, pageSize, statusString, user?.Id);
            pag ??= new LoanPaginationResultDto { Data = new List<LoanDto>(), PagesCount = 0 };

            var vm = new LoanPageViewModel
            {
                FilterDocumentId = documentId,
                FilterStatus = status,
                Loans = _mapper.Map<List<LoanViewModel>>(pag.Data),
                Page = page,
                MaxPages = pag.PagesCount,
                CurrentPage = page
            };

            var enumMappings = EnumMapper<LoanStatus>.GetAliasEnumPairs()
                .Select(e => new { Value = e.Value, Text = e.Alias })
                .ToList();
            ViewBag.Filters = new SelectList(enumMappings, "Value", "Text", statusString);

            return View(vm);
        }

        public async Task<IActionResult> Details(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return BadRequest();
            var dto = await _service.GetDetailed(publicId);
            if (dto == null) return NotFound();
            var vm = _mapper.Map<DetailedLoanViewModel>(dto);
            return View(vm);
        }

        public async Task<IActionResult> Edit(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return BadRequest();
            var dto = await _service.GetDetailed(publicId);
            if (dto == null) return NotFound();
            var vm = _mapper.Map<EditLoanViewModel>(dto);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(EditLoanViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Edit", vm);
            }

            await _service.UpdateLoanRate(vm.PublicId, vm.Rate);

            TempData["Success"] = "Tasa actualizada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Asign()
        {
            return RedirectToAction(nameof(Clients));
        }

        public async Task<IActionResult> Clients([FromQuery] string? documentIdFilter = null)
        {
            var dtos = await _service.GetClientsAvailableForLoan(documentIdFilter);
            var vm = new ClientsPageViewModel
            {
                Clients = _mapper.Map<List<UserViewModel>>(dtos),
                ClientsDebt = await _service.GetAverageLoanDebth(),
                DocumentIdFilter = documentIdFilter
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Terms(string clientId)
        {
            if (string.IsNullOrWhiteSpace(clientId))
            {
                TempData["Error"] = "Seleccionar un cliente es obligatorio.";
                return RedirectToAction(nameof(Clients));
            }

            var vm = new TermPageViewModel
            {
                ClientId = clientId
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetLoan(TermPageViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View("Terms", vm);
            }

            var request = new LoanRequest
            {
                AnualInterest = vm.AnnualInterestRate,
                ClientId = vm.ClientId,
                LoanAmount = vm.Amount,
                LoanTermInMonths = vm.TermInMonths
            };

            var result = await _service.HandleCreateRequest(request);

            if (result.ClientIsAlreadyHighRisk || result.ClientIsHighRisk)
            {
                if (result.ClientIsAlreadyHighRisk)
                    ViewBag.IsAlreadyHighRisk = "Este cliente se considera de alto riesgo, ya que su deuda actual supera el promedio del sistema.";
                else
                    ViewBag.IsHighRisk = "Asignar este préstamo convertirá al cliente en un cliente de alto riesgo, ya que su deuda superará el umbral promedio del sistema.";

                TempData["PendingLoanRequest"] = System.Text.Json.JsonSerializer.Serialize(request);
                return View("Warning", vm);
            }

            await _service.Create(request);
            TempData["Success"] = "Préstamo asignado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmLoan()
        {
            if (!TempData.TryGetValue("PendingLoanRequest", out var pending))
            {
                TempData["Error"] = "No hay solicitud pendiente para confirmar.";
                return RedirectToAction(nameof(Index));
            }

            var json = pending.ToString();
            var request = System.Text.Json.JsonSerializer.Deserialize<LoanRequest>(json);
            if (request == null)
            {
                TempData["Error"] = "Datos de solicitud inválidos.";
                return RedirectToAction(nameof(Index));
            }

            await _service.Create(request);
            TempData["Success"] = "Préstamo asignado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelWarning()
        {
            TempData.Remove("PendingLoanRequest");
            return RedirectToAction(nameof(Index));
        }
    }
}
