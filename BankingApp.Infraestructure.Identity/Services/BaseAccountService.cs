using BankingApp.Core.Application.Dtos.Email;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text;

namespace InvestmentApp.Infrastructure.Identity.Services
{
    public abstract class BaseAccountService : IBaseAccountService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        protected BaseAccountService(UserManager<AppUser> userManager, IEmailService emailService)
        {
            _userManager = userManager;
            _emailService = emailService;
        }

        public virtual async Task<RegisterUserResponseDto> RegisterUser(SaveUserDto saveDto, string? origin, bool? isApi = false)
        {
            RegisterUserResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                IsVerified = false,
                Roles = [],
                DocumentIdNumber = "",
                HasError = false,
                Errors = []
            };

            var userWithSameUserName = await _userManager.FindByNameAsync(saveDto.UserName);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"this username: {saveDto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(saveDto.Email);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"this email: {saveDto.Email} ya esta asociado al usuario.");
                return response;
            }

            AppUser user = new AppUser()
            {
                Name = saveDto.Name,
                LastName = saveDto.LastName,
                Email = saveDto.Email,
                UserName = saveDto.UserName,
                EmailConfirmed = false,
                IsActive = false, // Usuario inactivo hasta confirmar email
                DocumentIdNumber = saveDto.DocumentIdNumber,
            };

            var result = await _userManager.CreateAsync(user, saveDto.Password);
            if (result.Succeeded)
            {
                // Agregar todos los roles especificados
                foreach (var role in saveDto.Roles)
                {
                    var rol = EnumMapper<AppRoles>.FromString(role);
                    await _userManager.AddToRoleAsync(user, rol.ToString());
                }

                if (isApi != null && !isApi.Value)
                {
                    string verificationUri = await GetVerificationEmailUri(user, origin ?? "");

                    string emailBody = $@"
                        <html>
                        <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                            <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f8f9fa; border-radius: 10px;'>
                                <div style='background-color: #003d82; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                                    <h1 style='margin: 0;'>Banking App</h1>
                                </div>
                                <div style='background-color: white; padding: 30px; border-radius: 0 0 10px 10px;'>
                                    <h2 style='color: #003d82;'>¡Bienvenido {user.Name}!</h2>
                                    <p>Gracias por registrarte en Banking App. Para completar tu registro y activar tu cuenta, por favor confirma tu dirección de correo electrónico.</p>
                                    <div style='text-align: center; margin: 30px 0;'>
                                        <a href='{verificationUri}' style='background-color: #2c9fa3; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                            Confirmar mi cuenta
                                        </a>
                                    </div>
                                    <p style='color: #666; font-size: 14px;'>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                                    <p style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; word-break: break-all; font-size: 12px;'>{verificationUri}</p>
                                    <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                                    <p style='color: #999; font-size: 12px; text-align: center;'>Si no solicitaste esta cuenta, por favor ignora este correo.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                    await _emailService.SendAsync(new EmailRequestDto()
                    {
                        To = saveDto.Email,
                        BodyHtml = emailBody,
                        Subject = "Confirma tu cuenta - Banking App"
                    });
                }
                else
                {
                    string? verificationToken = await GetVerificationEmailToken(user);

                    // Email para API con formato HTML
                    string emailBody = $@"
                        <!DOCTYPE html>
                        <html lang='es'>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Confirmación de Cuenta - Banking App API</title>
                        </head>
                        <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f4f4f4;'>
                            <div style='max-width: 600px; margin: 40px auto; background-color: white; border-radius: 8px; overflow: hidden; box-shadow: 0 2px 8px rgba(0,0,0,0.1);'>
                                <div style='background: linear-gradient(135deg, #003d82 0%, #2c9fa3 100%); padding: 30px; text-align: center;'>
                                    <h1 style='color: white; margin: 0; font-size: 28px;'>Banking App API</h1>
                                    <p style='color: #e0f2f7; margin: 10px 0 0 0; font-size: 16px;'>Confirmación de Cuenta</p>
                                </div>
                                <div style='padding: 40px 30px;'>
                                    <h2 style='color: #003d82; margin-top: 0;'>¡Bienvenido a Banking App API!</h2>
                                    <p style='color: #333; line-height: 1.6; font-size: 16px;'>
                                        Tu cuenta ha sido creada exitosamente. Para activarla, necesitarás el siguiente token de verificación:
                                    </p>
                                    <div style='background-color: #f8f9fa; border-left: 4px solid #2c9fa3; padding: 20px; margin: 30px 0; border-radius: 4px;'>
                                        <p style='margin: 0 0 10px 0; color: #666; font-size: 14px; font-weight: bold;'>User ID:</p>
                                        <p style='margin: 0 0 20px 0; color: #003d82; font-size: 16px; font-family: monospace;'>{user.Id}</p>
                                        <p style='margin: 0 0 10px 0; color: #666; font-size: 14px; font-weight: bold;'>Token de Verificación:</p>
                                        <code style='display: block; background-color: white; padding: 15px; border: 2px solid #2c9fa3; border-radius: 4px; color: #003d82; font-size: 16px; word-break: break-all; font-family: monospace;'>
                                            {verificationToken}
                                        </code>
                                    </div>
                                    <p style='color: #666; line-height: 1.6; font-size: 14px;'>
                                        Utiliza este token en la solicitud POST a <code style='background-color: #f8f9fa; padding: 2px 6px; border-radius: 3px; color: #003d82;'>/account/confirm</code> para activar tu cuenta.
                                    </p>
                                    <div style='background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px;'>
                                        <p style='margin: 0; color: #856404; font-size: 14px;'>
                                            ⚠️ <strong>Importante:</strong> Este token es único y expirará en 24 horas. Mantenlo seguro.
                                        </p>
                                    </div>
                                    <hr style='border: none; border-top: 1px solid #e0e0e0; margin: 30px 0;'>
                                    <p style='color: #999; font-size: 12px; text-align: center;'>
                                        Si no solicitaste esta cuenta, por favor ignora este correo.
                                    </p>
                                </div>
                            </div>
                        </body>
                        </html>";

                    await _emailService.SendAsync(new EmailRequestDto()
                    {
                        To = saveDto.Email,
                        BodyHtml = emailBody,
                        Subject = "Confirmación de Cuenta - Banking App API"
                    });
                }

                var rolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.Roles = rolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }
        public virtual async Task<EditUserResponseDto> EditUser(SaveUserDto saveDto, string? origin, bool? isCreated = false, bool? isApi = false)
        {
            bool isNotcreated = !isCreated ?? false;
            EditUserResponseDto response = new()
            {
                Email = "",
                Id = "",
                LastName = "",
                Name = "",
                UserName = "",
                HasError = false,
                DocumentIdNumber = "",
                IsVerified = true,
                Errors = []
            };

            var userWithSameUserName = await _userManager.Users.FirstOrDefaultAsync(w => w.UserName == saveDto.UserName && w.Id != saveDto.Id);
            if (userWithSameUserName != null)
            {
                response.HasError = true;
                response.Errors.Add($"this username: {saveDto.UserName} is already taken.");
                return response;
            }

            var userWithSameEmail = await _userManager.Users.FirstOrDefaultAsync(w => w.Email == saveDto.Email && w.Id != saveDto.Id);
            if (userWithSameEmail != null)
            {
                response.HasError = true;
                response.Errors.Add($"this email: {saveDto.Email} is already taken.");
                return response;
            }

            var user = await _userManager.FindByIdAsync(saveDto.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this user");
                return response;
            }

            user.Name = saveDto.Name;
            user.LastName = saveDto.LastName;
            user.UserName = saveDto.UserName;
            user.EmailConfirmed = user.EmailConfirmed && user.Email == saveDto.Email;
            user.Email = saveDto.Email;
            user.DocumentIdNumber = saveDto.DocumentIdNumber;


            if (!string.IsNullOrWhiteSpace(saveDto.Password) && isNotcreated)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var resultChange = await _userManager.ResetPasswordAsync(user, token, saveDto.Password);

                if (resultChange != null && !resultChange.Succeeded)
                {
                    response.HasError = true;
                    response.Errors.AddRange(resultChange.Errors.Select(s => s.Description).ToList());
                    return response;
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
             

                if (!user.EmailConfirmed && isNotcreated)
                {
                    if (isApi != null && !isApi.Value)
                    {
                        string verificationUri = await GetVerificationEmailUri(user, origin ?? "");
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            BodyHtml = $"Please confirm your account visiting this URL {verificationUri}",
                            Subject = "Confirm registration"
                        });
                    }
                    else
                    {
                        string? verificationToken = await GetVerificationEmailToken(user);
                        await _emailService.SendAsync(new EmailRequestDto()
                        {
                            To = saveDto.Email,
                            BodyHtml = $"Please confirm your account use this token {verificationToken}",
                            Subject = "Confirm registration"
                        });
                    }
                }

                var updatedRolesList = await _userManager.GetRolesAsync(user);

                response.Id = user.Id;
                response.Email = user.Email ?? "";
                response.UserName = user.UserName ?? "";
                response.Name = user.Name;
                response.LastName = user.LastName;
                response.IsVerified = user.EmailConfirmed;
                response.Roles = updatedRolesList.ToList();

                return response;
            }
            else
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }
        }


    
        public virtual async Task<UserResponseDto> ForgotPasswordAsync(ForgotPasswordRequestDto request, bool? isApi = false)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByNameAsync(request.Username);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this username {request.Username}");
                return response;
            }

            user.EmailConfirmed = false;
            await _userManager.UpdateAsync(user);

            if (isApi != null && !isApi.Value)
            {
                var resetUri = await GetResetPasswordUri(user, request.Origin ?? "");

                string emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f8f9fa; border-radius: 10px;'>
                            <div style='background-color: #003d82; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                                <h1 style='margin: 0;'>Banking App</h1>
                            </div>
                            <div style='background-color: white; padding: 30px; border-radius: 0 0 10px 10px;'>
                                <h2 style='color: #003d82;'>Recuperación de Contraseña</h2>
                                <p>Hola {user.Name},</p>
                                <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta. Si no realizaste esta solicitud, puedes ignorar este correo.</p>
                                <div style='text-align: center; margin: 30px 0;'>
                                    <a href='{resetUri}' style='background-color: #2c9fa3; color: white; padding: 15px 30px; text-decoration: none; border-radius: 5px; display: inline-block; font-weight: bold;'>
                                        Restablecer mi contraseña
                                    </a>
                                </div>
                                <p style='color: #666; font-size: 14px;'>Si el botón no funciona, copia y pega este enlace en tu navegador:</p>
                                <p style='background-color: #f8f9fa; padding: 10px; border-radius: 5px; word-break: break-all; font-size: 12px;'>{resetUri}</p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                                <p style='color: #999; font-size: 12px; text-align: center;'>Este enlace expirará en 24 horas por seguridad.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    BodyHtml = emailBody,
                    Subject = "Restablecer contraseña - Banking App"
                });
            }
            else
            {
                string? resetToken = await GetResetPasswordToken(user);

                string emailBody = $@"
                    <html>
                    <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333;'>
                        <div style='max-width: 600px; margin: 0 auto; padding: 20px; background-color: #f8f9fa; border-radius: 10px;'>
                            <div style='background-color: #003d82; color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0;'>
                                <h1 style='margin: 0;'>Banking App API</h1>
                            </div>
                            <div style='background-color: white; padding: 30px; border-radius: 0 0 10px 10px;'>
                                <h2 style='color: #003d82;'>Token de Recuperación de Contraseña</h2>
                                <p>Hola {user.Name},</p>
                                <p>Recibimos una solicitud para restablecer la contraseña de tu cuenta. Utiliza el siguiente token para completar el proceso:</p>
                                <div style='background-color: #f8f9fa; padding: 20px; border-radius: 5px; text-align: center; margin: 30px 0; border: 2px dashed #2c9fa3;'>
                                    <p style='margin: 0; font-size: 12px; color: #666; margin-bottom: 10px;'>Tu token de reseteo:</p>
                                    <code style='font-size: 14px; font-weight: bold; color: #003d82; word-break: break-all; display: block;'>{resetToken}</code>
                                </div>
                                <p style='color: #666; font-size: 14px;'><strong>Importante:</strong> Copia este token y úsalo en la solicitud de reseteo de contraseña del API.</p>
                                <p style='color: #666; font-size: 14px;'><strong>Tu User ID es:</strong> {user.Id}</p>
                                <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
                                <p style='color: #999; font-size: 12px; text-align: center;'>Este token expirará pronto por seguridad. Si no solicitaste este cambio, ignora este correo.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendAsync(new EmailRequestDto()
                {
                    To = user.Email,
                    BodyHtml = emailBody,
                    Subject = "Token de Reseteo de Contraseña - Banking App API"
                });
            }

            return response;
        }

        public virtual async Task<UserResponseDto> ResetPasswordAsync(ResetPasswordRequestDto request)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(request.Id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this user");
                return response;
            }

            var token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(request.Token));
            var result = await _userManager.ResetPasswordAsync(user, token, request.Password);
            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors.AddRange(result.Errors.Select(s => s.Description).ToList());
                return response;
            }

            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return response;
        }
        public virtual async Task<UserResponseDto> DeleteAsync(string id)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };
            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                response.HasError = true;
                response.Errors.Add($"There is no acccount registered with this user");
                return response;
            }

            await _userManager.DeleteAsync(user);

            return response;
        }
        public virtual async Task<UserDto?> GetUserByEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                DocumentIdNumber = user.DocumentIdNumber,
                IsVerified = user.EmailConfirmed,
                Status = user.IsActive ? "Activo" : "Inactivo",
                IsActive = user.IsActive,
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }
        public virtual async Task<UserDto?> GetUserById(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            if (user == null)
            {
                return null;
            }
            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());
           

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                DocumentIdNumber = user.DocumentIdNumber,
                IsVerified = user.EmailConfirmed,
                Status = user.IsActive ? "Activo" : "Inactivo",
                IsActive = user.IsActive,
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }
        public virtual async Task<UserDto?> GetUserByUserName(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return null;
            }

            var rolesList = await _userManager.GetRolesAsync(user);
            var role = EnumMapper<AppRoles>.FromString(rolesList.First());

            var userDto = new UserDto()
            {
                Id = user.Id,
                Email = user.Email ?? "",
                LastName = user.LastName,
                Name = user.Name,
                UserName = user.UserName ?? "",
                DocumentIdNumber = user.DocumentIdNumber,
                IsVerified = user.EmailConfirmed,
                Status = user.IsActive ? "Activo" : "Inactivo",
                IsActive = user.IsActive,
                Role = EnumMapper<AppRoles>.ToString(role)
            };

            return userDto;
        }
        public virtual async Task<List<UserDto>> GetAllUser(bool? isActive = true)
        {
            List<UserDto> listUsersDtos = [];

            var users = _userManager.Users;

            if (isActive != null && isActive == true)
            {
                // Filtrar solo por IsActive (usuarios que han confirmado su correo)
                users = users.Where(w => w.IsActive);
            }

            var listUser = await users.ToListAsync();

            foreach (var item in listUser)
            {
                var rolesList = await _userManager.GetRolesAsync(item);

                // Usar directamente el string del rol sin conversión
                string roleName = rolesList.FirstOrDefault() ?? "UNKNOWN";

                listUsersDtos.Add(new UserDto()
                {
                    Id = item.Id,
                    Email = item.Email ?? "",
                    LastName = item.LastName,
                    Name = item.Name,
                    UserName = item.UserName ?? "",
                    DocumentIdNumber = item.DocumentIdNumber,
                    IsVerified = item.EmailConfirmed,
                    Status = item.IsActive ? "Activo" : "Inactivo",
                    IsActive = item.IsActive,
                    Role = roleName
                });
            }

            return listUsersDtos;
        }
        public virtual async Task<UserResponseDto> ConfirmAccountAsync(string userId, string token)
        {
            UserResponseDto response = new() { HasError = false, Errors = [] };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.Message = "There is no acccount registered with this user";
                response.HasError = true;
                return response;
            }

            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
            {
                // Activar el usuario al confirmar el email
                user.IsActive = true;
                await _userManager.UpdateAsync(user);

                response.Message = $"Cuenta confirmada para {user.Email}. Ahora puedes usar la aplicación";
                response.HasError = false;
                return response;
            }
            else
            {
                response.Message = $"Ocurrió un error al confirmar el correo {user.Email}";
                response.HasError = true;
                return response;
            }
        }

        #region "Protected methods"

        protected async Task<string> GetVerificationEmailUri(AppUser user, string origin)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ConfirmEmail";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var verificationUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri.ToString(), "token", token);

            return verificationUri;
        }

        protected async Task<string?> GetVerificationEmailToken(AppUser user)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        protected async Task<string> GetResetPasswordUri(AppUser user, string origin)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var route = "Login/ResetPassword";
            var completeUrl = new Uri(string.Concat(origin, "/", route));
            var resetUri = QueryHelpers.AddQueryString(completeUrl.ToString(), "userId", user.Id);
            resetUri = QueryHelpers.AddQueryString(resetUri.ToString(), "token", token);

            return resetUri;
        }

        protected async Task<string?> GetResetPasswordToken(AppUser user)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            return token;
        }
        #endregion

        public virtual async Task<EditUserResponseDto> UpdateUserStatusAsync(string userId, bool isActive)
        {
            EditUserResponseDto response = new()
            {
                HasError = false,
                Email = "",
                Id = "",
                IsVerified = false,
                LastName = "",
                Name = "",
                UserName = "",
                DocumentIdNumber = ""
            };

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                response.HasError = true;
                response.Errors = new List<string> { "El usuario no existe" };
                return response;
            }

            user.IsActive = isActive;
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                response.HasError = true;
                response.Errors = result.Errors.Select(e => e.Description).ToList();
                return response;
            }

            var rolesList = await _userManager.GetRolesAsync(user);

            response.Id = user.Id;
            response.Email = user.Email ?? "";
            response.UserName = user.UserName ?? "";
            response.Name = user.Name;
            response.LastName = user.LastName;
            response.IsVerified = user.EmailConfirmed;
            response.Roles = rolesList.ToList();

            return response;
        }
    }
}
