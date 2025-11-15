

namespace BankingApp.Core.Application.ViewModels.User
{
    public class UsersPageViewModel
    {
        public required List<UserViewModel> Users { get; set; }
        public int Page { get; set; }
        public int CurrentPage { get; set; }

        public string? Role { get; set; }

        public int MaxPages { get; set; }

    }
}
