using AutoMapper;
using BankingApp.Core.Application.Dtos.Commerce;
using BankingApp.Core.Application.Dtos.Common;
using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Runtime.InteropServices;

namespace BankingApi.Controllers.v1
{
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "ADMIN")]
    [Route("api/v{version::apiVersion}/commerce")]

    public class CommerceController : BaseApiController
    {
        private readonly ICommerceService _commerceService;
        private readonly IMapper _mapper ;
        private readonly IAccountServiceForWebApi _accountServiceForWeb;

        public CommerceController(ICommerceService commerceService, IMapper mapper, IAccountServiceForWebApi accountServiceForWeb)
        {
            _commerceService = commerceService;
            _mapper = mapper;
            _accountServiceForWeb = accountServiceForWeb;
        }
        [HttpGet(Name = "ObtenerTodosLoscomercios")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<IActionResult> GetAll([FromQuery] int page=1, [FromQuery] int pageSize=20)
        {
            try
            {
                var result = await _commerceService.GetAllActiveFiltered(page, pageSize);
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }




        }
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] int id)
        {
            try
            {
                var result = await _commerceService.GetByIdAsync(id);
                if (result == null) return NotFound("Comercio no encontrado");
                return Ok(JsonConvert.SerializeObject(result));

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] CreateCommerceDto dto)
        {

            if (string.IsNullOrEmpty(dto.Name) || dto.Name=="string") return BadRequest("El nombre es requerido");
            if (string.IsNullOrEmpty(dto.Description) || dto.Description== "string") return BadRequest("La descripcion es requerida");
            if (string.IsNullOrEmpty(dto.Logo) || dto.Logo == "string") return BadRequest("El logo es requerido");


            try
            {

            
                var result = await _commerceService.AddAsync(_mapper.Map<CommerceDto> (dto));

                return CreatedAtAction(nameof(Register), result);
             

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }

        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update([FromRoute]int id, [FromBody] EditCommerceDto dto)
        {

            if (string.IsNullOrEmpty(dto.Name)) return BadRequest("El nombre es requerido");
            if (string.IsNullOrEmpty(dto.Description)) return BadRequest("La descripcion es requerida");
            if (string.IsNullOrEmpty(dto.Logo)) return BadRequest("El logo es requerido");


            try
            {

                var commerce = await _commerceService.GetByIdAsync(id);
                if (commerce == null)
                    return NotFound("Comercio no encontrado");

                if (!string.IsNullOrWhiteSpace(dto.Logo))
                    commerce.Logo = dto.Logo;

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    commerce.Description = dto.Description;

                if (!string.IsNullOrWhiteSpace(dto.Name))
                    commerce.Name = dto.Name;

                var result = await _commerceService.UpdateAsync(id, _mapper.Map<CommerceDto>(commerce));
                return NoContent();

            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }





        }
            [HttpPatch("{id}")]
            public async Task<IActionResult> ToogleState([FromRoute] int id, [FromBody] StatusUpdateDto  updateStatusDto)
            {


                try
                {

                    var commerce = await _commerceService.GetByIdAsync(id);
                    if (commerce == null)
                        return NotFound("Comercio no encontrado");

                  commerce.IsActive = updateStatusDto.Status;

                  if (updateStatusDto.Status)
                {
                    var result = await _commerceService.UpdateAsync(id, _mapper.Map<CommerceDto>(commerce));

                }
                else
                {


                  var list = await _commerceService.GetCommerceAssociates(id);
                    await   _accountServiceForWeb.DeactivateUsersAsync(list);
                }

                return NoContent();

                }
                catch
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }


            
            }
    }
}
