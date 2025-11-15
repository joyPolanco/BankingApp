using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace BankingApp.Core.Application.LayerConfigurations
{
    public static class ServicesRegistration
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
           services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<ISavingAccountServiceForApi,SavingAccountServiceForApi>();
            services.AddScoped<ICommerceService,CommerceService> ();
            services.AddScoped<IBeneficiaryService, BeneficiaryService>();

            services.AddScoped<ILoanServiceForWebApi, LoanServiceForWebApi>();
            services.AddScoped<ILoanServiceForWebApp, LoanServiceForWebApp>();

            services.AddScoped(typeof(IGenericService<,>), typeof(GenericService<,>));
            services.AddScoped<ISavingAccountServiceForApi, SavingAccountServiceForApi>();
            services.AddScoped<ICommerceService, CommerceService>();
            services.AddScoped<ICreditCardService, CreditCardService>();
            services.AddScoped<ISavingsAccountServiceForWebApp, SavingsAccountServiceForWebApp>();

            services.AddScoped<ITransactionService, TransactionExpressService>();
            services.AddScoped<ITransactionToCreditCardService, TransactionToCreditCardService>();
            services.AddScoped<ITransactionToLoanService, TransactionToLoanService>();
            services.AddScoped<ITransactionToBeneficiaryService, TransactionExpressService>();


            services.AddScoped<ISavingAccountServiceForApi, SavingAccountServiceForApi>();
            services.AddScoped<IUserAccountManagementService, UserAccountManagmentService>();
            services.AddScoped<IDashboardsStatsService, DashboardStatsService>();


            services.AddAutoMapper(cfg => { }, Assembly.GetExecutingAssembly());
            EnumMappings.Initialize();
        }

    }
}

