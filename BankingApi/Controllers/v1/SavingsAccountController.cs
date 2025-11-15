using BankingApp.Core.Application.Dtos.Account;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BankingApi.Controllers.v1
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]
    [Route("api/v{version::apiVersion}/savings-account")]
    public class SavingsAccountController : BaseApiController
    {
        private readonly ISavingAccountServiceForApi _savingsAccountService;
        private readonly IUserService _userService;

        public SavingsAccountController(
            ISavingAccountServiceForApi savingsAccountService,
            IUserService userService)
        {
            _savingsAccountService = savingsAccountService;
            _userService = userService;
        }

        [HttpGet(Name = "ObtenerCuentasDeAhorro")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? cedula = null,
            [FromQuery] string? estado = null,
            [FromQuery] string? tipo = null)
        {
            try
            {
                // Validar el parámetro estado si se proporciona
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var estadosValidos = new[] { "activo", "cancelado" };
                    if (!estadosValidos.Contains(estado.ToLower()))
                    {
                        return BadRequest($"El estado '{estado}' no es válido. Los valores permitidos son: 'activo' o 'cancelado'");
                    }
                }

                // Validar el parámetro tipo si se proporciona
                if (!string.IsNullOrWhiteSpace(tipo))
                {
                    var tiposValidos = new[] { "principal", "secundaria" };
                    if (!tiposValidos.Contains(tipo.ToLower()))
                    {
                        return BadRequest($"El tipo '{tipo}' no es válido. Los valores permitidos son: 'principal' o 'secundaria'");
                    }
                }

                // Si se proporciona cédula, validar que el cliente exista
                string? clientId = null;
                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    var client = await _userService.GetByDocumentId(cedula);
                    if (client == null)
                    {
                        return NotFound($"No existe un cliente con la cédula '{cedula}'");
                    }
                    clientId = client.Id;
                }

                // Obtener todas las cuentas
                var allAccounts = await _savingsAccountService.GetAllList();

                if (allAccounts == null)
                {
                    return Ok(JsonConvert.SerializeObject(new
                    {
                        data = new List<object>(),
                        paginacion = new
                        {
                            paginaActual = page,
                            totalPaginas = 0,
                            totalRegistros = 0
                        }
                    }));
                }

                // Aplicar filtros
                var filteredAccounts = allAccounts.AsQueryable();

                if (clientId != null)
                {
                    filteredAccounts = filteredAccounts.Where(a => a.ClientId == clientId);
                }

                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var statusEnum = estado.ToLower() == "activo" ? AccountStatus.ACTIVE : AccountStatus.CANCELLED;
                    filteredAccounts = filteredAccounts.Where(a => a.Status == statusEnum);
                }

                if (!string.IsNullOrWhiteSpace(tipo))
                {
                    var typeEnum = tipo.ToLower() == "principal" ? AccountType.PRIMARY : AccountType.SECONDARY;
                    filteredAccounts = filteredAccounts.Where(a => a.Type == typeEnum);
                }

                // Ordenar por más reciente
                filteredAccounts = filteredAccounts.OrderByDescending(a => a.CreatedAt);

                // Paginación
                var totalRecords = filteredAccounts.Count();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
                var paginatedAccounts = filteredAccounts
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                // Obtener información de clientes
                var accountsWithClientInfo = new List<object>();
                foreach (var account in paginatedAccounts)
                {
                    var client = await _userService.GetUserById(account.ClientId);
                    accountsWithClientInfo.Add(new
                    {
                        numeroCuenta = account.Number,
                        nombreCliente = client?.Name ?? "",
                        apellidoCliente = client?.LastName ?? "",
                        balance = account.Balance,
                        tipoCuenta = account.Type == AccountType.PRIMARY ? "principal" : "secundaria",
                        estado = account.Status == AccountStatus.ACTIVE ? "activo" : "cancelado"
                    });
                }

                var response = new
                {
                    data = accountsWithClientInfo,
                    paginacion = new
                    {
                        paginaActual = page,
                        totalPaginas = totalPages,
                        totalRegistros = totalRecords
                    }
                };

                return Ok(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost(Name = "AsignarCuentaDeAhorro")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateSavingsAccountDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.ClientDocument))
            {
                missingFields.Add("cedulaCliente");
            }

            if (dto.InitialBalance < 0)
            {
                missingFields.Add("balanceInicial");
            }

            if (missingFields.Any())
            {
                return BadRequest(new
                {
                    mensaje = "Faltan uno o más campos requeridos.",
                    camposFaltantes = missingFields
                });
            }

            try
            {
                // Verificar que el cliente existe
                var client = await _userService.GetByDocumentId(dto.ClientDocument);
                if (client == null)
                {
                    return NotFound("El cliente especificado no existe");
                }

                // Verificar que el cliente tiene rol CLIENT (puede ser CLIENT, Client, cliente, etc.)
                var normalizedRole = client.Role?.Trim().ToUpperInvariant();
                if (normalizedRole != "CLIENT" && normalizedRole != "CLIENTE")
                {
                    return BadRequest($"Solo se pueden asignar cuentas a usuarios con rol de cliente. Rol actual: {client.Role}");
                }

                // Obtener el ID del admin logueado
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized("No se pudo identificar al administrador");
                }

                // Generar número de cuenta único
                var accountNumber = await _savingsAccountService.GenerateAccountNumber();

                // Crear la cuenta secundaria
                var account = new AccountDto
                {
                    Id = 0,
                    Number = accountNumber,
                    ClientId = client.Id,
                    Balance = dto.InitialBalance,
                    Type = AccountType.SECONDARY,
                    Status = AccountStatus.ACTIVE,
                    CreatedAt = DateTime.Now,
                    AdminId = adminId
                };

                var createdAccount = await _savingsAccountService.AddAsync(account);

                if (createdAccount == null)
                {
                    return BadRequest("No se pudo crear la cuenta de ahorro");
                }

                var response = new
                {
                    numeroCuenta = createdAccount.Number,
                    nombreCliente = client.Name,
                    apellidoCliente = client.LastName,
                    balance = createdAccount.Balance,
                    tipoCuenta = "secundaria",
                    estado = "activo"
                };

                return Created($"/api/v1/savings-account/{createdAccount.Number}", JsonConvert.SerializeObject(response));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ya existe"))
            {
                return Conflict("No se pudo generar un número de cuenta único. Por favor intente nuevamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{accountNumber}/transactions", Name = "ObtenerTransaccionesDeCuenta")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetTransactions([FromRoute] string accountNumber)
        {
            try
            {
                // Buscar la cuenta por número
                var accounts = await _savingsAccountService.GetAllList();
                var account = accounts?.FirstOrDefault(a => a.Number == accountNumber);

                if (account == null)
                {
                    return NotFound("La cuenta especificada no existe");
                }

                // TODO: Implementar obtención de transacciones cuando esté disponible el servicio
                // Por ahora retornamos una lista vacía
                var response = new
                {
                    transacciones = new List<object>()
                };

                return Ok(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
