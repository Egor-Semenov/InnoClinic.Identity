using InnoClinic.Identity.Data;
using InnoClinic.Identity.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace InnoClinic.Identity.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureSqlConnection(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddDbContext<AuthDbContext>(opts =>
                opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));
        }

        public static void ConfigureIdentityServer(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentityServer()
                .AddConfigurationStore(opt =>
                {
                    opt.ConfigureDbContext = c => c.UseSqlServer(configuration.GetConnectionString("sqlConnection"),
                        sql => sql.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name));
                })
                .AddOperationalStore(opt =>
                {
                    opt.ConfigureDbContext = o => o.UseSqlServer(configuration.GetConnectionString("sqlConnection"),
                        sql => sql.MigrationsAssembly(typeof(Program).GetTypeInfo().Assembly.GetName().Name));
                })
                .AddAspNetIdentity<AppUser>();
        }

        public static void ConfigureIdentityUsers(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, IdentityRole>(config =>
            {
                config.Password.RequiredLength = 4;
                config.Password.RequireDigit = false;
                config.Password.RequireUppercase = false;
                config.Password.RequireNonAlphanumeric = false;
            })
                .AddEntityFrameworkStores<AuthDbContext>()
                .AddDefaultTokenProviders();
        }
    }
}
