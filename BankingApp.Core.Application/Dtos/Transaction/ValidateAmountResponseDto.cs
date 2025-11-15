

namespace BankingApp.Core.Application.Dtos.Transaction
{
    public class ValidateAmountResponseDto
    {


        public bool IsSuccess { get; set; }
        public string? Error { get; set; }
        public int AccounId { get; set; }   

    }
}
