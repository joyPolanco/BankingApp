

namespace BankingApp.Core.Domain.Entities
{
    public class CommerceUser
    {
        public int Id { get; set; }
        public required int CommerceId { get; set; }
        public  Commerce? Commerce { get; set; }

        public required string UserId {  get; set; }
    }
}
