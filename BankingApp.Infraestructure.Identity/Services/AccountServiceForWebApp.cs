using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Infraestructure.Identity.Entities;
using InvestmentApp.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;

namespace BankingApp.Infraestructure.Identity.Services
{
    public class AccountServiceForWebAPP : BaseAccountService, IAccountServiceForWebAPP
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountServiceForWebAPP(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService) : base(userManager, emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }



        public async Task<LoginResponseDto> AuthenticateAsync(LoginDto loginDto)
        {

            LoginResponseDto responseDto = new LoginResponseDto() { Email = "", Id = "", UserName = "", HasError = false };
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                responseDto.HasError = true;
                responseDto.Error = $"No hay ningún usuario con el nombre de usuario {loginDto.Username}";
                return responseDto;
            }

            if (!user.EmailConfirmed)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Esta cuenta no está activa. Actívala mediante el link que ha sido enviado a tu correo";
                return responseDto;
            }

            if (!user.IsActive)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Esta cuenta está desactivada. Contacta al administrador para más información";
                return responseDto;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                responseDto.HasError = true;
                responseDto.Error = $"Usuario o contraseña incorrectos";
                return responseDto;

            }
            var rolesList = await _userManager.GetRolesAsync(user);
            responseDto.Id = user.Id;
            responseDto.UserName = user.UserName ?? "";
            responseDto.Email = user.Email ?? "";
            responseDto.IsVerified = user.EmailConfirmed && user.IsActive;
            responseDto.Roles = rolesList.ToList();


            return responseDto;
        }
        public async Task ToogleState(string userId, string origin)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.EmailConfirmed = !user.EmailConfirmed;

            await _userManager.UpdateAsync(user);
           
        }
        public async Task SignOutAsync()
        {
            await _signInManager.SignOutAsync();
        }


    }
}
