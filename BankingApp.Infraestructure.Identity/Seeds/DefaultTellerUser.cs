using BankingApp.Core.Domain.Common.Enums;
using BankingApp.Infraestructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankingApp.Infraestructure.Identity.Seeds
{
    public static class DefaultTellerUser
    {


        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
          
           
            //Agregando un usuario de tipo Teller 
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
