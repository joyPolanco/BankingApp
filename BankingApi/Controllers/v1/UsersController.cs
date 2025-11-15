using AutoMapper;
using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Dtos.Common;
using BankingApp.Core.Application.Dtos.Operations;
using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data;


namespace BankingApi.Controllers.v1
{

    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]
    [Route("api/v{version::apiVersion}/users")]

    public class UsersController : BaseApiController
    {
        private IUserService _userService;
        private readonly IAccountServiceForWebApi _accountService;
        private readonly IUserAccountManagementService _bankAccountService;
        private readonly ICommerceService _commerceService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IAccountServiceForWebApi accountService, IUserAccountManagementService bankAccountUserService, ICommerceService commerceService, IMapper mapper)
        {
            _userService = userService;
            _accountService = accountService;
            _bankAccountService = bankAccountUserService;
            _commerceService = commerceService;
            _mapper = mapper;
        }

        [HttpGet(Name = "ObtenerTodosLosUsuarios")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, int pageSize = 20, string? rol = null)
        {
            try
            {
                var result = await _userService.GetAllExceptCommerce(page, pageSize, rol);
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }




        }

        [HttpGet("commerce", Name = "ObtenerUsuariosComercios")]
        public async Task<IActionResult> GetAllCommerce([FromQuery] int page = 1, int pageSize = 20, string? rol = null)
        {
            try
            {
                var result = await _userService.GetAllOnlyCommerce(page, pageSize, rol);
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }




        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost(Name = "CrearUsuario")]
        public async Task<IActionResult> Register([FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud.");

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.UserName) || dto.UserName == "string") missingFields.Add("usuario");
            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string") missingFields.Add("correo");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string") missingFields.Add("contraseña");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.ConfirmPassword == "string") missingFields.Add("confirmarContraseña");
            if (string.IsNullOrWhiteSpace(dto.Role) || dto.Role == "string") missingFields.Add("rol");
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name == "string") missingFields.Add("nombre");
            if (string.IsNullOrWhiteSpace(dto.LastName) || dto.LastName == "string") missingFields.Add("apellido");
            if (string.IsNullOrWhiteSpace(dto.DocumentIdNumber) || dto.DocumentIdNumber == "string") missingFields.Add("cédula");

            if (missingFields.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Faltan uno o más campos requeridos.",
                    camposFaltantes = missingFields
                });
            }

            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Las contraseñas no coinciden.");

            if (!new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest("El formato del correo electrónico no es válido.");

            if (dto.DocumentIdNumber.Length != 11 || !dto.DocumentIdNumber.All(char.IsDigit))
                return BadRequest("La cédula debe contener exactamente 11 caracteres numéricos.");

            var commerceAliases = EnumMapper<AppRoles>.GetAliasesFor(AppRoles.COMMERCE)
                 .Select(a => a.ToLower())
                 .ToList();

            var allRoles = EnumMapper<AppRoles>.GetAllAliases()
                .Select(a => a.ToLower())
                .Where(a => !commerceAliases.Contains(a))
                .Distinct()
                .ToList();

            if (!allRoles.Contains(dto.Role.ToLower()))
                return BadRequest("Rol inválido.");

            try
            {
                var enumOption = EnumMapper<AppRoles>.FromString(dto.Role);
                RegisterUserWithAccountResponseDto result = new();

                if (enumOption == AppRoles.CLIENT)
                {
                    var currentUser = await _userService.GetCurrentUserAsync();

                    if (currentUser == null)
                        return StatusCode(StatusCodes.Status401Unauthorized, "No se pudo identificar al usuario actual.");
                    dto.Roles = new List<string> { enumOption.ToString() };

                    result = await _bankAccountService.CreateUserWithAmount(dto, currentUser.Id, true);
                }
                else
                {
                    var saveUserDto = new SaveUserDto
                    {
                        UserName = dto.UserName,
                        Email = dto.Email,
                        LastName = dto.LastName,
                        Name = dto.Name,
                        Password = dto.Password,
                        DocumentIdNumber = dto.DocumentIdNumber,
                        Roles = new List<string> { enumOption.ToString() }
                    };

                    var resultRegister = await _accountService.RegisterUser(saveUserDto, null, true);

                    if (resultRegister.HasError)
                        return Conflict("El nombre de usuario o el correo electrónico ya están registrados.");

                    result = _mapper.Map<RegisterUserWithAccountResponseDto>(resultRegister);
                    result.IsSuccesful = true;
                }

                if (result == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al registrar el usuario.");

                if (!result.IsSuccesful && result.IsInternalError)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error del servidor.");

                if (!result.IsSuccesful)
                    return Conflict("El nombre de usuario o el correo electrónico ya están registrados.");

                return CreatedAtAction(nameof(Register), new { dto.UserName }, new
                {
                    mensaje = "Usuario creado exitosamente.",
                    usuario = dto.UserName,
                    rol = dto.Role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error del servidor: {ex.Message}");
            }
        }

        [HttpPost("commerce/{commerceId}", Name = "CrearUsuarioComercio")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegisterUserCommerce([FromRoute] int? commerceId, [FromBody] CreateUserDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud.");

            // Validar campos vacíos o por defecto
            var missingFields = new List<string>();
            if (string.IsNullOrWhiteSpace(dto.UserName) || dto.UserName == "string") missingFields.Add("usuario");
            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string") missingFields.Add("correo");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string") missingFields.Add("contraseña");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.ConfirmPassword == "string") missingFields.Add("confirmar contraseña");
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name == "string") missingFields.Add("nombre");
            if (string.IsNullOrWhiteSpace(dto.LastName) || dto.LastName == "string") missingFields.Add("apellido");
            if (string.IsNullOrWhiteSpace(dto.DocumentIdNumber) || dto.DocumentIdNumber == "string") missingFields.Add("cédula");

            if (missingFields.Any())
                return BadRequest(new { mensaje = "Faltan campos requeridos.", camposFaltantes = missingFields });

            // Validaciones de formato
            if (dto.Password != dto.ConfirmPassword)
                return BadRequest("Las contraseñas no coinciden.");

            if (!new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest("Correo con formato incorrecto.");

            if (dto.DocumentIdNumber.Length != 11 || !dto.DocumentIdNumber.All(char.IsDigit))
                return BadRequest("La cédula debe contener 11 caracteres numéricos.");

            if (commerceId is null)
                return BadRequest("El ID del comercio es requerido.");

            try
            {
                var commerce = await _commerceService.GetByIdAsync(commerceId.Value);
                if (commerce == null)
                    return BadRequest("El comercio especificado no existe.");

                if (commerce.UserId != null)
                    return BadRequest("El comercio ya tiene un usuario asociado.");

                var allRoles = EnumMapper<AppRoles>.GetAllAliases().Select(a => a.ToLower()).Distinct().ToList();
                if (!allRoles.Contains(dto.Role.ToLower()))
                    return BadRequest("Rol inválido.");

                var enumRole = EnumMapper<AppRoles>.FromString(dto.Role);
                var currentUser = await _userService.GetCurrentUserAsync();
                if (enumRole != AppRoles.COMMERCE)
                    return BadRequest("Rol inválido");

                dto.Roles = new List<string> { "commerce" };

                //Validar que no tenga usuario
                var commerceHasUser = await _commerceService.CommerceAlreadyHasUser(commerceId ?? 0);

                if (commerceHasUser) return BadRequest("El comercio ya tiene un usuario asociado");

                // Crear usuario
                var result = await _bankAccountService.CreateUserWithAmount(dto, currentUser?.Id ?? "", true);
                if (result == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, "Error interno al crear el usuario.");

                if (!result.IsSuccesful && !result.IsInternalError)
                    return Conflict("Usuario o correo ya registrado.");

                var resultCommerce = await _commerceService.SetUser(commerceId.Value, result.EntityId);

                if (resultCommerce.IsSuccessful)
                {
                    return CreatedAtRoute("CrearUsuarioComercio", new { commerceId }, new
                    {
                        mensaje = "Usuario de comercio creado correctamente.",
                        usuarioId = result.EntityId,
                        comercioId = commerceId
                    });
                }
                if (!result.IsSuccesful && !resultCommerce.IsInternalError)
                {
                    return BadRequest("El comercio ya tiene un usuario asociado ");
                }


                return StatusCode(StatusCodes.Status500InternalServerError, "Ocurrió un error en el servidor");


            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpPut("{id}", Name = "ActualizarUsuario")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateUserDto dto)
        {


            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("El ID del usuario es requerido.");

            dto.Id = id;

            var missingFields = new List<string>();
            if (string.IsNullOrWhiteSpace(dto.UserName) || dto.UserName == "string") missingFields.Add("usuario");
            if (string.IsNullOrWhiteSpace(dto.Email) || dto.Email == "string") missingFields.Add("correo");
            if (string.IsNullOrWhiteSpace(dto.Name) || dto.Name == "string") missingFields.Add("nombre");
            if (string.IsNullOrWhiteSpace(dto.LastName) || dto.LastName == "string") missingFields.Add("apellido");
            if (string.IsNullOrWhiteSpace(dto.DocumentIdNumber) || dto.DocumentIdNumber == "string") missingFields.Add("cédula");
            if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password == "string") missingFields.Add("Contraseña");
            if (string.IsNullOrWhiteSpace(dto.ConfirmPassword) || dto.ConfirmPassword == "string") missingFields.Add("ConfirmarContraseña");

            if (missingFields.Any())
                return BadRequest(new { mensaje = "Faltan campos requeridos.", camposFaltantes = missingFields });

            if (!new EmailAddressAttribute().IsValid(dto.Email))
                return BadRequest("Correo con formato incorrecto.");

            if (dto.DocumentIdNumber.Length != 11 || !dto.DocumentIdNumber.All(char.IsDigit))
                return BadRequest("La cédula debe contener 11 caracteres numéricos.");

            if (!string.IsNullOrWhiteSpace(dto.Password) && dto.Password != dto.ConfirmPassword)
                return BadRequest("Las contraseñas no coinciden.");

            try
            {
                var existingUser = await _accountService.GetUserById(id);
                if (existingUser == null)
                    return NotFound("El usuario especificado no existe.");

                var result = await _accountService.EditUser(new SaveUserDto
                {
                    Id = id,
                    UserName = dto.UserName,
                    Email = dto.Email,
                    LastName = dto.LastName,
                    Name = dto.Name,
                    Password = dto.Password ?? string.Empty,
                    DocumentIdNumber = dto.DocumentIdNumber
                }, null, false, true);

                if (result == null || result.HasError)
                    return Conflict(new { mensaje = "Error al actualizar el usuario", errores = result?.Errors });

                var role = EnumMapper<AppRoles>.FromString(existingUser.Role);
                if (role == AppRoles.CLIENT)
                {
                    if (!dto.AdditionalBalance.HasValue || dto.AdditionalBalance<0)
                        return BadRequest("Debe especificar 'montoAdicional' para clientes y debe ser mayor o igual a 0.");

                  
                    if (dto.AdditionalBalance > 0)
                        await _bankAccountService.ChangeBalanceForClient(result.Id, dto.AdditionalBalance.Value);
                }

                return NoContent( );
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }



        [HttpPatch("{id}/status", Name = "CambiarEstadoUsuario")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]

        public async Task<IActionResult> UpdateStatus([FromRoute] string id, [FromBody] StatusUpdateDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("El ID del usuario es requerido");
            }


            if (dto.Status == null)
            {
                return BadRequest("Estructura inválida");

            }
            try


            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser.Id == id)
                    return StatusCode(StatusCodes.Status403Forbidden,
                        new { message = "Un usuario no puede modificar su propio estado" });

                var updateResult = await _accountService.UpdateUserStatusAsync(id, dto.Status);

                if (updateResult.HasError)
                {
                    if (updateResult.Errors!.Any())
                    {
                        return NotFound("El usuario especificado no existe");
                    }



                    return BadRequest(new
                    {
                        mensaje = "Error al actualizar el estado del usuario",
                        errores = updateResult.Errors
                    });
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "ObtenerUsuarioPorId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest("El ID del usuario es requerido");
            }

            try
            {
                var user = await _accountService.GetUserById(id);
                if (user == null)
                {
                    return NotFound("El usuario especificado no existe");
                }
                var userRol = EnumMapper<AppRoles>.FromString(user.Role);

                if (userRol==AppRoles.COMMERCE || userRol == AppRoles.CLIENT)
                {
                    var userPrimaryAccount = await _bankAccountService.GetMainSavingAccount(user.Id);

                    if (userPrimaryAccount != null)
                    {


                        user.MainAccount = _mapper.Map<PrimaryAccountDto>(userPrimaryAccount);
                    }
                }
                return Ok(user);


            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

    }
}