using InnoClinic.Identity.Extensions;

namespace InnoClinic.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ConfigureSqlConnection(builder.Configuration);
            builder.Services.ConfigureIdentityUsers();
            builder.Services.ConfigureIdentityServer();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.MapDefaultControllerRoute();

            app.Run();
        }
    }
}