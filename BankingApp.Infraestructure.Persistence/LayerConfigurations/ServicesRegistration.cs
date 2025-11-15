using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Interfaces;
using BankingApp.Infraestructure.Persistence.Contexts;
using BankingApp.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BankingApp.Infraestructure.Persistence.LayerConfigurations
{
    public static class ServicesRegistration
    {
        public static void AddPersistenceLayer(this IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<BankingContext>(options =>
                    options.UseInMemoryDatabase("BankingDB"));
            }
            else
            {
                services.AddDbContext<BankingContext>(options =>
                    options.UseSqlServer(
                        config.GetConnectionString("DefaultConnection"),
                        m => m.MigrationsAssembly(typeof(BankingContext).Assembly.FullName)));
            }
            #endregion

            #region Repositories
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAccountRepository, AccountRepository>();
            services.AddScoped<ICommerceRepository, CommerceRepository>();
            services.AddScoped<ILoanRepository, LoanRepository>();
            services.AddScoped<ICreditCardRepository, CreditCardRepository>();
            services.AddScoped<IPurchaseRepository, PurchaseRepository>();
            services.AddScoped<IBeneficiaryRepository, BeneficiaryRepository>();
            services.AddScoped<IInstallmentRepository, InstallmentRepository>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<ICommerceRepository, CommerceRepository>();
            services.AddScoped<ICommerceUserRepository, CommerceUserRepository>();

            services.AddScoped<ITransacctionRepository,TransacctionRepository>();

            #endregion
        }
    }
}
