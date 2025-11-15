using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.ViewModels.User;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace BankingApp.Controllers
{
    public class LoginController : Controller
    {
        private readonly IAccountServiceForWebAPP _accountServiceForWebApp;
        private readonly UserManager<AppUser> _userManager;
      

        public LoginController(IAccountServiceForWebAPP accountServiceForWebApp, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _accountServiceForWebApp = accountServiceForWebApp;
            _userManager = userManager;
         
        }

        public async Task<IActionResult> Index()
        {
            // Si ya hay un usuario autenticado
            if (User.Identity?.IsAuthenticated == true)
            {
                AppUser? userSession = await _userManager.GetUserAsync(User);

                if (userSession != null)
                {
                    // Verificar que el usuario esté activo y confirmado
                    if (userSession.IsActive && userSession.EmailConfirmed)
                    {
                        var roles = await _userManager.GetRolesAsync(userSession);
                        if (roles.Any())
                        {
                            return RedirectToHome(roles.First());
                        }
                    }

                    // Si el usuario no está activo o no tiene email confirmado, cerrar sesión
                    await _accountServiceForWebApp.SignOutAsync();
                }
            }

            return View(new LoginViewModel() { Password = "", UserName = "" });
        }


        [HttpPost]
        public async Task<IActionResult> Index(LoginViewModel vm)
        {
            // Si ya hay un usuario autenticado
            if (User.Identity?.IsAuthenticated == true)
            {
                AppUser? userSession = await _userManager.GetUserAsync(User);

                if (userSession != null)
                {
                    // Verificar que el usuario esté activo y confirmado
                    if (userSession.IsActive && userSession.EmailConfirmed)
                    {
                        var roles = await _userManager.GetRolesAsync(userSession);
                        if (roles.Any())
                        {
                            return RedirectToHome(roles.First());
                        }
                    }

                    // Si el usuario no está activo o no tiene email confirmado, cerrar sesión
                    await _accountServiceForWebApp.SignOutAsync();
                }
            }


            if (!ModelState.IsValid)
            {
                vm.Password = "";
                return View(vm);
            }

            // Validaciones adicionales
            if (string.IsNullOrWhiteSpace(vm.UserName))
            {
                vm.HasError = true;
                vm.Error = "El nombre de usuario no puede estar vacío o contener solo espacios";
                vm.Password = "";
                return View(vm);
            }

            if (string.IsNullOrWhiteSpace(vm.Password))
            {
                vm.HasError = true;
                vm.Error = "La contraseña no puede estar vacía o contener solo espacios";
                vm.Password = "";
                return View(vm);
            }

            if (vm.UserName.Length < 3)
            {
                vm.HasError = true;
                vm.Error = "El nombre de usuario debe tener al menos 3 caracteres";
                vm.Password = "";
                return View(vm);
            }

            if (vm.Password.Length < 6)
            {
                vm.HasError = true;
                vm.Error = "La contraseña debe tener al menos 6 caracteres";
                vm.Password = "";
                return View(vm);
            }

            LoginResponseDto userDto = await _accountServiceForWebApp.AuthenticateAsync(new LoginDto()
            {
                Password = vm.Password,
                Username = vm.UserName
            });

            if (userDto != null && !userDto.HasError)
            {
                // Usar el rol del DTO de autenticación
                if (userDto.Roles != null && userDto.Roles.Any())
                {
                    return RedirectToHome(userDto.Roles.First());
                }
                return RedirectToRoute(new { controller = "Login", action = "Index" });
            }
            else
            {
                vm.HasError = true;
                vm.Error = userDto?.Error ?? "Error al iniciar sesión";
                vm.Password = "";
                return View(vm);
            }
        }

        private IActionResult RedirectToHome(string role)
        {
            return role.ToUpper() switch
            {
                "ADMIN" => RedirectToRoute(new { area = "Admin", controller = "Home", action = "Index" }),
                "TELLER" => RedirectToRoute(new { area = "Teller", controller = "Home", action = "Index" }),
                "CLIENT" => RedirectToRoute(new { area = "Client", controller = "Home", action = "Index" }),
                _ => RedirectToRoute(new { controller = "Login", action = "Index" })
            };
        }

        public async Task<IActionResult> Logout()
        {
            await _accountServiceForWebApp.SignOutAsync();
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public async Task<IActionResult> AccessDenied()
        {
            // Si hay un usuario autenticado, cerrar su sesión
            if (User.Identity?.IsAuthenticated == true)
            {
                await _accountServiceForWebApp.SignOutAsync();
            }

            TempData["ErrorMessage"] = "No tienes permisos para acceder a esta página o tu sesión ha expirado. Por favor, inicia sesión nuevamente.";
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            UserResponseDto response = await _accountServiceForWebApp.ConfirmAccountAsync(userId, token);
            return View("ConfirmEmail", response.Message);
        }

        public IActionResult ForgotPassword()
        {
            return View(new ForgotPasswordViewModel() { UserName = "" });
        }

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            string origin = $"{Request.Scheme}://{Request.Host}";

            ForgotPasswordRequestDto dto = new() { Username = vm.UserName, Origin = origin };

            UserResponseDto returnUser = await _accountServiceForWebApp.ForgotPasswordAsync(dto);

            if (returnUser.HasError)
            {
                vm.HasError = true;
                vm.Error = string.Join(", ", returnUser.Errors ?? new List<string>());
                return View(vm);
            }

            TempData["Success"] = "Se ha enviado un correo con las instrucciones para restablecer tu contraseña.";
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }

        public IActionResult ResetPassword(string userId, string token)
        {
            return View(new ResetPasswordViewModel() { UserId = userId, Token = token, Password = "", ConfirmPassword = "" });
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.Password = "";
                vm.ConfirmPassword = "";
                return View(vm);
            }

            ResetPasswordRequestDto dto = new() { Id = vm.UserId, Password = vm.Password, Token = vm.Token };

            UserResponseDto returnUser = await _accountServiceForWebApp.ResetPasswordAsync(dto);

            if (returnUser.HasError)
            {
                vm.HasError = true;
                vm.Error = string.Join(", ", returnUser.Errors ?? new List<string>());
                vm.Password = "";
                vm.ConfirmPassword = "";
                return View(vm);
            }

            TempData["Success"] = "Tu contraseña ha sido restablecida exitosamente.";
            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }




        //metodo privado para redireccionar
        private IActionResult RedirectByRole(string role)
        {
            if (role.Contains("ADMIN"))
                return RedirectToRoute(new { area = "Admin", controller = "Home", action = "Index" });

            if (role.Contains("CLIENT"))
                return RedirectToRoute(new { area = "Client", controller = "Home", action = "Index" });

            if (role.Contains("TELLER"))
                return RedirectToRoute(new { area = "Teller", controller = "Home", action = "Index" });

            return RedirectToRoute(new { controller = "Login", action = "Index" });
        }
    }
}
