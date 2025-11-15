using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;


namespace BankingApp.Infraestructure.Identity.Seeds
{
    public static class DefaultClientUser
    {


        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {

            //agregando un usuario de tipo cliente
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
                Name = "NewUserClient",
                UserName = "Client_new",
                Email = "kelvinkodaddiaz@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var Tuser = await userManager.FindByNameAsync("Client_new");
            await userManager.AddPasswordAsync(Tuser!, "Pas@cli2");
            await userManager.AddToRoleAsync(Tuser!, AppRoles.CLIENT.ToString());




            await userManager.CreateAsync(new AppUser
            {
                DocumentIdNumber = "",
                LastName = "",
                Name = "Three",
                UserName = "Client3",
                Email = "kervindiazramirez@gmail.com",
                EmailConfirmed = true,
                IsActive = true

            });

            var Newuser = await userManager.FindByNameAsync("Client3");
            await userManager.AddPasswordAsync(Newuser!, "Cli$Pee1");
            await userManager.AddToRoleAsync(Newuser!, AppRoles.CLIENT.ToString());

        }





    }
}
