using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Core.Application.Dtos.User
{
    public class RegisterUserWithAccountResponseDto
    {
        public  string Id { get; set; } = string.Empty;
        public  string UserName { get; set; } = string.Empty;
        public  string Email { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public  string Name { get; set; } = string.Empty;
        public  string LastName { get; set; } = string.Empty;
        public  string DocumentIdNumber { get; set; } = string.Empty;

        public bool UserAlreadyHasAccount { get; set; } = false;

        public bool IsSuccesful { get; set; }
        public string ?StatusMessage { get; set; }
        public bool IsInternalError { get; set; }
        public string EntityId { get; set; }
        public bool UserAlreadyExists { get; internal set; }
    }
}
