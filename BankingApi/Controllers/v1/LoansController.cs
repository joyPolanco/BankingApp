using BankingApp.Core.Application.Dtos.Loan;
using BankingApp.Core.Application.Interfaces;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;


namespace BankingApi.Controllers.v1
{
    [Route("api/v{version::apiVersion}/loan")]

    public class LoansController : BaseApiController
    {
        private readonly ILoanServiceForWebApi _loanService;
        private readonly IUserService _userService;
        
        public LoansController(ILoanServiceForWebApi loanService, IUserService userService)
        {
            _loanService = loanService;
            _userService = userService;
        }

        [HttpGet(Name = "GetAllLoans")]
        public async Task<IActionResult> GetAll (int page = 1, int pageSize = 20, string? state = null, string? DocumentId = null)
        {
            string? clientId = null;

            if (!string.IsNullOrWhiteSpace(DocumentId))
            {
                var user = await _userService.GetByDocumentId(DocumentId);
                if (user == null) return BadRequest("No existe ningun usuario asociado a esa cedula");
                clientId = user.DocumentIdNumber;
            }
            var all=await  _loanService.GetAllFiltered(page, pageSize, state,clientId);

            return Ok(all);
        }

        [HttpPost(Name = "SetLoan")]
        public async Task<IActionResult> SetLoan(LoanRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Faltan uno o más parámetros requeridos en la solicitud");
            }

            if (string.IsNullOrWhiteSpace(request.ClientId))
            {
                return BadRequest("El ID del usuario es requerido");
            }

            var user = await _userService.GetUserById(request.ClientId);
            if (user == null) return BadRequest("No existe ningun usuario asociado a ese Id");

            
                var requestResult = await _loanService.HandleCreateRequestApi(request);
                if (requestResult.ClientHasActiveLoan) return BadRequest("El usuario ya tiene un prestamo activo");
                if (requestResult.ClientIsHighRisk) return Conflict("El usuario es de alto riesgo");
            if (requestResult.LoanCreated) return Created();

            else return BadRequest();
            
        }


        [HttpGet("{id}", Name = "GetLoanDetails")]
        public async Task<IActionResult> GetDetails([FromRoute]string id)
        {

            var result = await _loanService.GetDetailed(id);
            if (result == null) return NotFound();

            return Ok(result);
        }


        [HttpPatch("{id}/rate", Name = "UpdateLoanRate")]
        public async Task<IActionResult> SetRate([FromRoute]string publicId ,[FromBody] decimal rate)
        {

            if (rate <= 0) return BadRequest("Tasa invalida");

            
            if (string.IsNullOrEmpty(publicId) || publicId == "string")
            {
                return BadRequest();
            }
            var result = await _loanService.UpdateLoanRate(publicId,rate);
            if (!result.IsSuccessful) return NotFound();
            if (result.IsSuccessful) return NoContent();
            if (result == null) return NotFound();

            return Ok(result);
        }
    }
}
