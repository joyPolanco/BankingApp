using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Seeds
{
    public static class DefaultAdminUser
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "",
                LastName = "",
                Name = "",
                UserName = "superAdmin",
                Email = "no.repply.bankingapp@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var user = await  userManager.FindByNameAsync("superAdmin");
            await userManager.AddPasswordAsync(user!, "Pa$Word1");

            await userManager.AddToRoleAsync(user!, AppRoles.ADMIN.ToString());






            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "",
                LastName = "",
                Name = "",
                UserName = "UltraAdmin",
                Email = "no.repply.bankingapp@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var user2 = await userManager.FindByNameAsync("UltraAdmin");
            await userManager.AddPasswordAsync(user2!, "Pa$Word1");

            await userManager.AddToRoleAsync(user2!, AppRoles.ADMIN.ToString());








            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "",
                LastName = "",
                Name = "BeeThree",
                UserName = "Client02",
                Email = "Client.bankingapp@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var _user = await userManager.FindByNameAsync("Client02");
            await userManager.AddPasswordAsync(_user!, "Cli$Pas1");
            await userManager.AddToRoleAsync(_user!, AppRoles.CLIENT.ToString());


         
            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "",
                LastName = "",
                Name = "DIKEY",
                UserName = "TellerUser02",
                Email = "Teller02.bankingapp@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });


            var Teller = await userManager.FindByNameAsync("TellerUser02");
            await userManager.AddPasswordAsync(Teller!, "Tel$Pas2");
            await userManager.AddToRoleAsync(Teller!, AppRoles.TELLER.ToString());

        }
    }
}
