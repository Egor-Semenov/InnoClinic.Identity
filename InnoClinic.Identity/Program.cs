using InnoClinic.Identity.Extensions;
using InnoClinic.Identity.RabbitMQ.Interfaces;
using InnoClinic.Identity.RabbitMQ.Producers;
using InnoClinic.Identity.RabbitMQ.Subscribers;

namespace InnoClinic.Identity
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.ConfigureSqlConnection(builder.Configuration);
            builder.Services.ConfigureIdentityUsers();
            builder.Services.ConfigureIdentityServer(builder.Configuration);

            builder.Services.AddScoped<IMessageProducer, PatientCreatedProducer>();
            builder.Services.AddHostedService<UserCreatedSubscriber>();
            builder.Services.AddHostedService<UserDeletedSubscriber>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();
            app.MapDefaultControllerRoute();

            app.MigrateDatabase();
            app.Run();
        }
    }
}