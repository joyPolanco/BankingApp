using BankingApp.Core.Application.Dtos.CreditCard;
using BankingApp.Core.Application.Helpers;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Common.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Security.Claims;

namespace BankingApi.Controllers.v1
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]
    [Route("api/v{version::apiVersion}/creditcards")]
    public class CreditCardsController : BaseApiController
    {
        private readonly ICreditCardService _creditCardService;
        private readonly IUserService _userService;

        public CreditCardsController(ICreditCardService creditCardService, IUserService userService)
        {
            _creditCardService = creditCardService;
            _userService = userService;
        }

        [HttpGet(Name = "ObtenerTodasLasTarjetas")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? estado = null, [FromQuery] string? cedula = null)
        {
            try
            {
                // Validar el parámetro estado si se proporciona
                if (!string.IsNullOrWhiteSpace(estado))
                {
                    var estadosValidos = new[] { "activas", "canceladas" };
                    if (!estadosValidos.Contains(estado.ToLower()))
                    {
                        return BadRequest($"El estado '{estado}' no es válido. Los valores permitidos son: 'activas' o 'canceladas'");
                    }
                }

                // Si se proporciona cédula, validar que el cliente exista
                if (!string.IsNullOrWhiteSpace(cedula))
                {
                    var client = await _userService.GetByDocumentId(cedula);
                    if (client == null)
                    {
                        return NotFound($"No existe un cliente con la cédula '{cedula}'");
                    }

                    var result = await _creditCardService.GetByClientDocumentAsync(cedula, estado);
                    return Ok(JsonConvert.SerializeObject(result));
                }

                // Si no hay cédula, obtener todas las tarjetas
                var allCards = await _creditCardService.GetAllAsync(page, pageSize, estado);
                return Ok(JsonConvert.SerializeObject(allCards));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}", Name = "ObtenerDetallesDeTarjeta")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById([FromRoute] string id)
        {
            try
            {
                CreditCardDto? card = null;

                // Intentar buscar por ID numérico
                if (int.TryParse(id, out int numericId))
                {
                    card = await _creditCardService.GetByIdAsync(numericId);
                }

                // Si no se encontró por ID, intentar buscar por número de tarjeta
                if (card == null)
                {
                    card = await _creditCardService.GetByNumberAsync(id);
                }

                if (card == null)
                {
                    return NotFound("La tarjeta especificada no existe");
                }

                var purchases = await _creditCardService.GetPurchasesByCardIdAsync(card.Id);

                var response = new
                {
                    consumos = purchases
                };

                return Ok(JsonConvert.SerializeObject(response));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost(Name = "AsignarTarjetaDeCredito")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateCreditCardDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            var missingFields = new List<string>();

            if (string.IsNullOrWhiteSpace(dto.ClientId))
            {
                missingFields.Add("clienteId");
            }

            if (dto.CreditLimitAmount <= 0)
            {
                missingFields.Add("limite");
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
                var client = await _userService.GetUserById(dto.ClientId);
                if (client == null)
                {
                    return NotFound("El cliente especificado no existe");
                }

                // Verificar que el cliente tiene rol CLIENT
                if (!client.Role.Equals("CLIENT", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest("Solo se pueden asignar tarjetas a usuarios con rol de cliente");
                }

                // Obtener el ID del admin logueado
                var adminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(adminId))
                {
                    return Unauthorized("No se pudo identificar al administrador");
                }

                var result = await _creditCardService.CreateAsync(dto.ClientId, dto.CreditLimitAmount, adminId);

                return Created($"/api/v1/creditcards/{result.Id}", JsonConvert.SerializeObject(result));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("ya existe"))
            {
                return Conflict("No se pudo generar un número de tarjeta único. Por favor intente nuevamente.");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}/limit", Name = "ActualizarLimiteTarjeta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute] string id, [FromBody] UpdateCreditCardDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            var missingFields = new List<string>();

            if (dto.CreditLimitAmount <= 0)
            {
                missingFields.Add("nuevoLimite");
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
                CreditCardDto? card = null;

                // Intentar buscar por ID numérico
                if (int.TryParse(id, out int numericId))
                {
                    card = await _creditCardService.GetByIdAsync(numericId);
                }

                // Si no se encontró por ID, intentar buscar por número de tarjeta
                if (card == null)
                {
                    card = await _creditCardService.GetByNumberAsync(id);
                }

                if (card == null)
                {
                    return NotFound("La tarjeta especificada no existe");
                }

                // Verificar que el nuevo límite no sea menor que la deuda actual
                if (dto.CreditLimitAmount < card.TotalAmountOwed)
                {
                    return BadRequest("El nuevo límite no puede ser inferior al monto adeudado actual");
                }

                var updated = await _creditCardService.UpdateCreditLimitAsync(card.Id, dto.CreditLimitAmount);
                if (!updated)
                {
                    return BadRequest("No se pudo actualizar el límite de la tarjeta");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPatch("{id}/cancel", Name = "CancelarTarjeta")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Cancel([FromRoute] string id)
        {
            try
            {
                CreditCardDto? card = null;

                // Intentar buscar por ID numérico
                if (int.TryParse(id, out int numericId))
                {
                    card = await _creditCardService.GetByIdAsync(numericId);
                }

                // Si no se encontró por ID, intentar buscar por número de tarjeta
                if (card == null)
                {
                    card = await _creditCardService.GetByNumberAsync(id);
                }

                if (card == null)
                {
                    return NotFound("La tarjeta especificada no existe");
                }

                // Verificar que no tenga deuda pendiente
                if (card.TotalAmountOwed > 0)
                {
                    return BadRequest("Para cancelar esta tarjeta, el cliente debe saldar la totalidad de la deuda pendiente");
                }

                var cancelled = await _creditCardService.CancelCardAsync(card.Id);
                if (!cancelled)
                {
                    return BadRequest("No se pudo cancelar la tarjeta");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
