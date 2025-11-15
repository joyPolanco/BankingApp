using BankingApp.Core.Application.Dtos.User;
using BankingApp.Core.Application.Interfaces;
using BankingApp.Core.Domain.Settings;
using BankingApp.Infraestructure.Identity.Contexts;
using BankingApp.Infraestructure.Identity.Entities;
using BankingApp.Infraestructure.Identity.Seeds;
using BankingApp.Infraestructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Security.Claims;
using System.Text;


namespace BankingApp.Infraestructure.Identity.LayerConfigurations
{
    public static class ServicesRegistration
    {
        public static void AddIdentityLayerIocForWebApp(this IServiceCollection services, IConfiguration config)
        {

            GeneralConfiguration(services, config);
            #region Identity

            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;

                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;


            });
            services.AddIdentityCore<AppUser>()
                        .AddRoles<IdentityRole>()
                        .AddSignInManager()
                        .AddEntityFrameworkStores<IdentityContext>()
                        .AddDefaultTokenProviders();



            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);

            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;

            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(5);
                opt.LoginPath = "/Login/Index";
                opt.AccessDeniedPath = "/Login/AccessDenied";
                opt.SlidingExpiration = true;
            });

            // Configurar para incluir roles en claims
            services.AddScoped<Microsoft.AspNetCore.Identity.IUserClaimsPrincipalFactory<AppUser>,
                Microsoft.AspNetCore.Identity.UserClaimsPrincipalFactory<AppUser, IdentityRole>>();
            #endregion


            #region Services
            services.AddScoped<IAccountServiceForWebAPP, AccountServiceForWebAPP>();
            services.AddScoped<IUserService, IdentityUserService>();

            #endregion

        }
        public static void AddIdentityLayerIocForWebApi(this IServiceCollection services, IConfiguration config)
        {

            GeneralConfiguration(services, config);
            #region Identity

            #region Configuration 
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

            #endregion
            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;

                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;


            });
            services.AddIdentityCore<AppUser>()
                        .AddRoles<IdentityRole>()
                        .AddSignInManager()
                        .AddEntityFrameworkStores<IdentityContext>()
                        .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);



            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);

            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;


            }).AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2),
                    ValidIssuer = config["JwtSettings:Issuer"],
                    ValidAudience = config["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"] ?? "")),

                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = ClaimTypes.Role
                };
                opt.RequireHttpsMetadata = false;

                opt.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = af =>
                    {
                        af.NoResult();
                        af.Response.StatusCode = 500;
                        af.Response.ContentType = "text/plain";
                        return af.Response.WriteAsync(af.Exception.Message.ToString());

                    },
                    OnChallenge = c =>
                    {
                        c.HandleResponse();
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new JwtResponseDto() { HasError = true, Error = "Token ausente o inválido" });
                        return c.Response.WriteAsync(result);
                    },
                    OnForbidden = c =>
                    {
                        c.Response.StatusCode = 403;
                        c.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new JwtResponseDto() { HasError = true, Error = "Usuario sin permisos" });
                        return c.Response.WriteAsync(result);
                    }
                };
            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(30);


            });

            #endregion


            #region Services
            services.AddScoped<IAccountServiceForWebApi, AccountServiceForWebApi>();
            services.AddScoped<IAccountServiceForWebAPP, AccountServiceForWebAPP>();
            services.AddScoped<IUserService, IdentityUserService>();

            #endregion

        }

        private static async Task GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {

            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityContext>(opt =>
                                                        opt.UseInMemoryDatabase("AppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("IdentityConnection");
                services.AddDbContext<IdentityContext>(

                    (serviceProvider, opt) =>
                    {
                        opt.EnableSensitiveDataLogging();
                        opt.UseSqlServer(connectionString,
                            m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                    );
            }


        }

        public static async Task RunIdentitySeedAsync(this IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var servicesProvider = scope.ServiceProvider;

            var userManager = servicesProvider.GetRequiredService<UserManager<AppUser>>();
            var roleManager = servicesProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await DefaultRoles.SeedAsync(roleManager);

            await DefaultAdminUser.SeedAsync(userManager);
            await DefaultClientUser.SeedAsync(userManager);
            await DefaultTellerUser.SeedAsync(userManager);
            await DefaultUser.SeedAsync(userManager);
        }
    }
}
