using InnoClinic.Identity.Configurations;
using InnoClinic.Identity.Data;
using InnoClinic.Identity.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Identity.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureSqlConnection(this IServiceCollection services, IConfiguration configuration) 
        {
            services.AddDbContext<AuthDbContext>(opts =>
                opts.UseSqlServer(configuration.GetConnectionString("sqlConnection")));
        }

        public static void ConfigureIdentityServer(this IServiceCollection services)
        {
            services.AddIdentityServer()
                .AddInMemoryApiScopes(Configuration.ApiScopes)
                .AddInMemoryApiResources(Configuration.ApiResources)
                .AddInMemoryIdentityResources(Configuration.IdentityResources)
                .AddInMemoryClients(Configuration.Clients)
                .AddDeveloperSigningCredential()
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
