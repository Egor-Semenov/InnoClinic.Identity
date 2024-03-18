using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Mappers;
using InnoClinic.Identity.Configurations;
using Microsoft.EntityFrameworkCore;

namespace InnoClinic.Identity
{
    public static class MigrationManager
    {
        public static WebApplication MigrateDatabase(this WebApplication webApp)
        {
            using (var scope = webApp.Services.CreateScope())
            {
                scope.ServiceProvider.GetRequiredService<PersistedGrantDbContext>().Database.Migrate();
                using var context = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
                try
                {
                    context.Database.Migrate();
                    if (!context.Clients.Any())
                    {
                        foreach (var client in Configuration.Clients)
                        {
                            context.Clients.Add(client.ToEntity());
                        }
                        context.SaveChanges();
                    }
                    if (!context.IdentityResources.Any())
                    {
                        foreach (var resource in Configuration.IdentityResources)
                        {
                            context.IdentityResources.Add(resource.ToEntity());
                        }
                        context.SaveChanges();
                    }
                    if (!context.ApiScopes.Any())
                    {
                        foreach (var apiScope in Configuration.ApiScopes)
                        {
                            context.ApiScopes.Add(apiScope.ToEntity());
                        }
                        context.SaveChanges();
                    }
                    if (!context.ApiResources.Any())
                    {
                        foreach (var resource in Configuration.ApiResources)
                        {
                            context.ApiResources.Add(resource.ToEntity());
                        }
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    //Log errors or do anything you think it's needed
                    throw;
                }
            }
            return webApp;
        }
    }
}
