using AutoMapper;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.LayerConfigurations;
using BankingApp.Core.Application.ViewModels.User;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace BankingApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "ADMIN")]
    [Area("Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly IUserAccountManagementService _managementService;
        private readonly IAccountServiceForWebAPP _accountService;

        public UsersController(IUserService userService, IMapper mapper, IUserAccountManagementService managementService, IAccountServiceForWebAPP accountServiceForWebAPP)
        {
            _userService = userService;
            _mapper = mapper;
            _managementService = managementService;
            _accountService = accountServiceForWebAPP;
        }

        public async Task<IActionResult> Index([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? filter = null)
        {
            var dtos = await _userService.GetAllExceptCommerce(page, pageSize, filter);

            var vm = new UsersPageViewModel
            {
                Role = filter,
                Users = _mapper.Map<List<UserViewModel>>(dtos.Data),
                CurrentPage = page,
                MaxPages = dtos.PagesCount
            };

            var enumMappings = EnumMapper<AppRoles>.GetAliasEnumPairs()
                .Where(r => r.Value != AppRoles.COMMERCE)
                .Select(e => new { Value = e.Alias, Text = e.Alias })
                .ToList();

            ViewBag.Filters = new SelectList(enumMappings, "Value", "Text", filter);

            return View(vm);
        }



        [HttpPost]
        public async Task<IActionResult> Filter(UsersPageViewModel vm)
        {
            int step = 20;

            var dtos = await _userService.GetAllExceptCommerce(vm.Page, step, vm.Role);

            vm.Users = _mapper.Map<List<UserViewModel>>(dtos.Data);
            vm.MaxPages = dtos.PagesCount;

            var enumMappings = EnumMapper<AppRoles>.GetAliasEnumPairs()
                .Select(e => new { Value = e.Value, Text = e.Alias })
                .ToList();

            ViewBag.Filters = new SelectList(enumMappings, "Value", "Text");

            return View("Index", vm);
        }

        [HttpGet("activate")]

        public IActionResult Activate(string id)
        {
            ViewBag.IsDeactivateMode = false;
            return View("ChangeState", id);

        }
        [HttpGet("deactivate")]

        public IActionResult Deactivate(string id)
        {
            ViewBag.IsDeactivateMode = true;
            return View("ChangeState", id);

        }

        [HttpPost]

        public async Task<IActionResult> ToogleStatus(string UserId)
        {
            await _userService.ToogleState(UserId);
            return RedirectToAction("Index");
        }
        public IActionResult Register()
        {
            var enumMappings = EnumMapper<AppRoles>.GetAliasEnumPairs()
                          .Where(r => r.Value != AppRoles.COMMERCE)
                          .Select(e => new { Value = e.Value, Text = e.Alias })
                          .ToList();

            ViewBag.Roles = new SelectList(enumMappings, "Value", "Text");

            var vm = new CreateUserViewModel
            {
                Role = 0
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterPost(CreateUserViewModel vm)
        {
            var dto = _mapper.Map<CreateUserDto>(vm);
            var user = await _userService.GetCurrentUserAsync();

            var enumMappings = EnumMapper<AppRoles>.GetAliasEnumPairs()
                          .Where(r => r.Value != AppRoles.COMMERCE)
                          .Select(e => new { Value = e.Value, Text = e.Alias })
                          .ToList();

            ViewBag.Roles = new SelectList(enumMappings, "Value", "Text");
            var origin = Request.Headers.Origin.ToString();

            if (vm.Role == AppRoles.CLIENT)
            {
                if (!vm.InitialAmount.HasValue)
                {
                    vm.HasError = true;
                    vm.Error = "El monto inicial es requerido para clientes.";
                    return View("Register", vm);
                }
                dto.Roles = new List<string> { vm.Role.ToString().ToLower() };
                var result = await _managementService.CreateUserWithAmount(dto, user?.Id ?? user.Id,false,origin);
                if (result.UserAlreadyExists || !result.IsSuccesful)
                {
                    vm.HasError = true;
                    vm.Error = "Usuario o correo existentes";
                    return View("Register", vm);
                }

                return RedirectToAction("Index");
            }
            else
            {
                var saveDto = _mapper.Map<SaveUserDto>(vm);
                saveDto.Roles.Add(vm.Role.ToString().ToLower());

                var registerResult = await _accountService.RegisterUser(saveDto, origin, false);

                if (registerResult.HasError)
                {
                    vm.HasError = true;
                    vm.Error = "Usuario o correo existentes";
                    return View("Register", vm);
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Edit(string userId)
        {

            var user = await _userService.GetUserById(userId);

            bool isClient = user != null && EnumMapper<AppRoles>.FromString(user.Role.ToLower()) == AppRoles.CLIENT;

            var vm = _mapper.Map<EditUserWithAmountViewModel>(user);
            vm.IsClient = isClient;

            ViewBag.UserIsClient = isClient;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> EditPost(EditUserWithAmountViewModel vm)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            bool isClient = currentUser != null && currentUser.Role.ToLower() == AppRoles.CLIENT.ToString().ToLower();

            if (isClient && (vm.AditionalAmount == null || vm.AditionalAmount <= 0))
            {
                ModelState.AddModelError("AditionalAmount", "El monto adicional es obligatorio para clientes.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.UserIsClient = isClient;
                return View("Edit", vm);
            }

            var dto = _mapper.Map<UpdateUserDto>(vm);
            var result = await _managementService.EditUserAndAmountAsync(dto, currentUser?.Id ?? "");

            if (!result.IsSuccesful)
            {
                vm.HasError = true;
                vm.Error = "No se pudo actualizar el usuario.";
            }

            return View("Edit", vm);
        }
    }
}
