using BankingApp.Core.Application.Dtos.Login;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Settings;
using BankingApp.Infraestructure.Identity.Entities;
using InvestmentApp.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Identity.Services
{
    public class AccountServiceForWebApi : BaseAccountService, IAccountServiceForWebApi
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly SignInManager<AppUser> _signInManager;



        public AccountServiceForWebApi(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IEmailService emailService, IOptions<JwtSettings> jwtSettings) : base(userManager, emailService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _jwtSettings = jwtSettings.Value;
        }

        public override Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        {
            return base.ForgotPasswordAsync(request, isApi);
        }
        public override Task<RegisterUserResponseDto> RegisterUser(SaveUserDto saveDto, string? origin, bool? isApi = false)
        {
            return base.RegisterUser(saveDto, null, isApi);
        }
        public override Task<EditUserResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false)
        {
            return base.EditUser(saveDto, null, isCreated, isApi);
        }
        public async Task<LoginResponseForApi> AuthenticateAsync(LoginDto loginDto)
        {

            LoginResponseForApi responseDto = new LoginResponseForApi() { LastName = "", Name = "", HasError = false };
            var user = await _userManager.FindByNameAsync(loginDto.Username);
            if (user == null)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add($"No hay ningun usuario el nombre de usuario {loginDto.Username}");
                return responseDto;
            }
            if (!user.EmailConfirmed)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add($"Esta cuenta no esta activa. Actívala mediante un link que ha sido enviado a tu correo");
                return responseDto;
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName ?? "", loginDto.Password, false, false);
            if (!result.Succeeded)
            {
                responseDto.HasError = true;
                responseDto.Errors.Add($"Usuario o contraseña incorrectos");
                return responseDto;

            }
            JwtSecurityToken jwtSecurityToken = await GenerateJWToken(user);

            var rolesList = await _userManager.GetRolesAsync(user);


            responseDto.AccessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

            return responseDto;
        }

        public async Task DeactivateUsersAsync(List<string> userIds)
        {
            var usuarios = await _userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            foreach (var usuario in usuarios)
            {
                usuario.IsActive = false;

                await _userManager.UpdateAsync(usuario);
            }
        }




        #region private methods
        private async Task<JwtSecurityToken> GenerateJWToken(AppUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var rolesClaims = new List<Claim>();
            foreach (var role in roles)
            {
                rolesClaims.Add(new Claim("roles", role));
            }
            var claims = new[]
            {
                new Claim (JwtRegisteredClaimNames.Sub,user.UserName?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim ("uid", user.Id)
            }.Union(userClaims).Union(rolesClaims);

            var simetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var signingCredentials = new SigningCredentials(simetricSecurityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: signingCredentials



                );
            return token;
        }
        #endregion
    }
}
