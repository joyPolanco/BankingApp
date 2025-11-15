using AutoMapper;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.SavingsAccount;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BankingApp.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "ADMIN")]
    public class SavingsAccountController : Controller
    {
        private readonly ISavingsAccountServiceForWebApp _savingsAccountService;
        private readonly IUserService _userService;
        private readonly ICreditCardService _creditCardService;
        private readonly IAccountServiceForWebAPP _accountService;
        private readonly IMapper _mapper;

        public SavingsAccountController(
            ISavingsAccountServiceForWebApp savingsAccountService,
            IUserService userService,
            ICreditCardService creditCardService,
            IAccountServiceForWebAPP accountService,
            IMapper mapper)
        {
            _savingsAccountService = savingsAccountService;
            _userService = userService;
            _creditCardService = creditCardService;
            _accountService = accountService;
            _mapper = mapper;
        }

        // GET: Admin/SavingsAccount
        public async Task<IActionResult> Index(int page = 1, string? cedula = null, string? estado = null, string? tipo = null)
        {
            try
            {
                const int pageSize = 20;

                if (!string.IsNullOrEmpty(cedula))
                {
                    cedula = cedula.Trim().Replace("-", "");
                    var user = await _userService.GetByDocumentId(cedula);

                    if (user == null)
                    {
                        TempData["Warning"] = $"No se encontró ningún cliente con la cédula '{cedula}'. Verifique el número e intente nuevamente.";
                        ViewBag.Cedula = cedula;
                        ViewBag.Estado = estado;
                        ViewBag.Tipo = tipo;
                        ViewBag.CurrentPage = 1;
                        ViewBag.TotalPages = 0;
                        return View(new List<SavingsAccountViewModel>());
                    }

                    ViewBag.UserName = $"{user.Name} {user.LastName}";
                }

                var allAccounts = await _savingsAccountService.GetAllAccountsAsync(1, int.MaxValue, cedula, estado, tipo);
                
                var totalRecords = allAccounts.Count;
                var paginatedAccounts = allAccounts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var viewModels = _mapper.Map<List<SavingsAccountViewModel>>(paginatedAccounts);

                // Obtener nombres de clientes
                foreach (var vm in viewModels)
                {
                    var user = await _userService.GetUserById(vm.ClientId);
                    if (user != null)
                    {
                        vm.ClientName = $"{user.Name} {user.LastName}";
                    }
                }

                ViewBag.Cedula = cedula;
                ViewBag.Estado = estado;
                ViewBag.Tipo = tipo;
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                ViewBag.PageSize = pageSize;
                ViewBag.TotalRecords = totalRecords;

                return View(viewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar las cuentas de ahorro: " + ex.Message;
                return View(new List<SavingsAccountViewModel>());
            }
        }

        // GET: Admin/SavingsAccount/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var accountDto = await _savingsAccountService.GetAccountByIdAsync(id);
                
                if (accountDto == null)
                {
                    TempData["Error"] = "La cuenta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SavingsAccountViewModel>(accountDto);
                
                var user = await _userService.GetUserById(accountDto.UserId);
                if (user != null)
                {
                    viewModel.ClientName = $"{user.Name} {user.LastName}";
                }

                // Obtener las transacciones de la cuenta
                var transactions = await _savingsAccountService.GetAccountTransactionsAsync(id);

                var detailsViewModel = new SavingsAccountDetailsViewModel
                {
                    Account = viewModel,
                    Transactions = transactions
                };

                return View(detailsViewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/SavingsAccount/SelectClient
        public async Task<IActionResult> SelectClient(string? cedula = null)
        {
            try
            {
                var allUsers = await _accountService.GetAllUser(isActive: true); // Solo usuarios activos
                var clients = allUsers.Where(u => u.Role == "CLIENT").ToList();

                if (!string.IsNullOrEmpty(cedula))
                {
                    cedula = cedula.Trim().Replace("-", "");
                    clients = clients.Where(u => u.DocumentIdNumber.Replace("-", "").Contains(cedula)).ToList();
                }

                var clientViewModels = new List<ClientSelectionViewModel>();

                foreach (var client in clients)
                {
                    // Calcular deuda total del cliente desde las tarjetas
                    var clientCards = await _creditCardService.GetAllAsync(1, 10000, "ALL");
                    var userCards = clientCards.Where(c => c.ClientId == client.Id).ToList();
                    var totalDebt = userCards.Any() ? userCards.Sum(c => c.TotalAmountOwed) : 0;
                    
                    clientViewModels.Add(new ClientSelectionViewModel
                    {
                        Id = client.Id,
                        DocumentId = client.DocumentIdNumber,
                        FullName = $"{client.Name} {client.LastName}",
                        Email = client.Email,
                        TotalDebt = totalDebt
                    });
                }

                ViewBag.Cedula = cedula;
                return View(clientViewModels);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los clientes: " + ex.Message;
                return View(new List<ClientSelectionViewModel>());
            }
        }

        // POST: Admin/SavingsAccount/SelectClient
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmClient(string? selectedClientId)
        {
            if (string.IsNullOrEmpty(selectedClientId))
            {
                TempData["Error"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(SelectClient));
            }

            return RedirectToAction(nameof(Create), new { clientId = selectedClientId });
        }

        // GET: Admin/SavingsAccount/Create
        public async Task<IActionResult> Create(string clientId)
        {
            try
            {
                var user = await _userService.GetUserById(clientId);
                
                if (user == null)
                {
                    TempData["Error"] = "El cliente seleccionado no existe";
                    return RedirectToAction(nameof(SelectClient));
                }

                // Calcular deuda total del cliente
                var clientCards = await _creditCardService.GetAllAsync(1, 10000, "ALL");
                var userCards = clientCards.Where(c => c.ClientId == clientId).ToList();
                var totalDebt = userCards.Any() ? userCards.Sum(c => c.TotalAmountOwed) : 0;

                var viewModel = new SaveSavingsAccountViewModel
                {
                    ClientId = clientId,
                    ClientName = $"{user.Name} {user.LastName}",
                    ClientDocumentId = user.DocumentIdNumber,
                    TotalDebt = totalDebt,
                    InitialBalance = 0
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el formulario: " + ex.Message;
                return RedirectToAction(nameof(SelectClient));
            }
        }

        // POST: Admin/SavingsAccount/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveSavingsAccountViewModel viewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var user = await _userService.GetUserById(viewModel.ClientId);
                    if (user != null)
                    {
                        viewModel.ClientName = $"{user.Name} {user.LastName}";
                        viewModel.ClientDocumentId = user.DocumentIdNumber;
                        
                        var clientCards = await _creditCardService.GetAllAsync(1, 10000, "ALL");
                        var userCards = clientCards.Where(c => c.ClientId == viewModel.ClientId).ToList();
                        viewModel.TotalDebt = userCards.Any() ? userCards.Sum(c => c.TotalAmountOwed) : 0;
                    }
                    return View(viewModel);
                }

                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                
                var accountDto = _mapper.Map<BankingApp.Core.Application.Dtos.Account.AccountDto>(viewModel);
                accountDto.Balance = viewModel.InitialBalance;

                await _savingsAccountService.CreateSecondaryAccountAsync(accountDto, adminId!);

                TempData["Success"] = "Cuenta de ahorro secundaria asignada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al crear la cuenta: " + ex.Message;
                
                var user = await _userService.GetUserById(viewModel.ClientId);
                if (user != null)
                {
                    viewModel.ClientName = $"{user.Name} {user.LastName}";
                    viewModel.ClientDocumentId = user.DocumentIdNumber;
                    
                    var clientCards = await _creditCardService.GetAllAsync(1, 10000, "ALL");
                    var userCards = clientCards.Where(c => c.ClientId == viewModel.ClientId).ToList();
                    viewModel.TotalDebt = userCards.Any() ? userCards.Sum(c => c.TotalAmountOwed) : 0;
                }
                
                return View(viewModel);
            }
        }

        // GET: Admin/SavingsAccount/Cancel/5
        public async Task<IActionResult> Cancel(int id)
        {
            try
            {
                var accountDto = await _savingsAccountService.GetAccountByIdAsync(id);
                
                if (accountDto == null)
                {
                    TempData["Error"] = "La cuenta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = _mapper.Map<SavingsAccountViewModel>(accountDto);
                
                var user = await _userService.GetUserById(accountDto.UserId);
                if (user != null)
                {
                    viewModel.ClientName = $"{user.Name} {user.LastName}";
                }

                return View(viewModel);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la cuenta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/SavingsAccount/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            try
            {
                var account = await _savingsAccountService.GetAccountByIdAsync(id);
                
                if (account == null)
                {
                    TempData["Error"] = "La cuenta especificada no existe";
                    return RedirectToAction(nameof(Index));
                }

                var success = await _savingsAccountService.CancelAccountAsync(id);

                if (!success)
                {
                    TempData["Error"] = "No se pudo cancelar la cuenta. Solo se pueden cancelar cuentas secundarias.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["Success"] = "Cuenta cancelada exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cancelar la cuenta: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
