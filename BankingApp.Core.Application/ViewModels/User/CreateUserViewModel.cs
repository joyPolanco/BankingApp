using BankingApp.Core.Domain.Common.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace BankingApp.Core.Application.ViewModels.User
{
    public class CreateUserViewModel
    {



            [Required(ErrorMessage = "El nombre es requerido")]
            public string Name { get; set; } = string.Empty;

            [Required(ErrorMessage = "El apellido es requerido")]
            public string LastName { get; set; } = string.Empty;

            [Required(ErrorMessage = "La cédula es requerida")]
            public string DocumentIdNumber { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo electrónico es requerido")]
            [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "El nombre de usuario es requerido")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es requerida")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Confirmar contraseña es requerido")]
            [DataType(DataType.Password)]
            [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
            public string ConfirmPassword { get; set; } = string.Empty;
       
        
        [Required(ErrorMessage = "El rol requerido")]

        public required AppRoles Role { get; set; }
             public decimal? InitialAmount { get; set; } // Monto inicial para clientes


            public bool HasError { get; set; }
            public string? Error { get; set; }

    }
}



