

namespace BankingApp.Core.Application.Dtos.Loan
{
    public class PayLoanResponseDto
    {

        public Guid IdLoan { get; set; }
        public bool HasError {  get; set; }
        public string? Error { get; set; }   



    }
}
