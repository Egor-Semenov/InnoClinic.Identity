using InnoClinic.Identity.Models.Entities;
using InnoClinic.Identity.RabbitMQ.Models.Receive;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace InnoClinic.Identity.RabbitMQ.Subscribers
{
    public sealed class UserCreatedSubscriber : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        private const string Queue = "identity-service/user-created";

        public UserCreatedSubscriber(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;

            var connectionFactory = new ConnectionFactory
            {
                HostName = "localhost"
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += async (sender, eventArgs) =>
            {
                var contentArray = eventArgs.Body.ToArray();
                var contentString = Encoding.UTF8.GetString(contentArray);
                var message = JsonConvert.DeserializeObject<UserCreatedModel>(contentString);

                var user = new AppUser
                {
                    UserName = message.Username,
                    Id = message.UserId.ToString(),
                    IsPasswordConfirmed = false
                };

                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var result = await userManager.CreateAsync(user, message.Password);
                if (!result.Succeeded)
                {
                    _channel.BasicNack(eventArgs.DeliveryTag, false, false);
                }
                else
                {
                    result = await userManager.AddToRoleAsync(user, message.Role);
                    if (!result.Succeeded)
                    {
                        await userManager.DeleteAsync(user);
                        _channel.BasicNack(eventArgs.DeliveryTag, false, false);
                    }
                    else
                    {
                        //await SendEmailToCreatedUser(message!);
                        _channel.BasicAck(eventArgs.DeliveryTag, false);
                    }
                }
            };
            _channel.BasicConsume(Queue, false, consumer);

            return Task.CompletedTask;
        }

        private async Task SendEmailToCreatedUser(UserCreatedModel user)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("InnoClinic", Environment.GetEnvironmentVariable("InnoClinic.Identity.Email")));
            message.To.Add(new MailboxAddress("", user.Email));
            message.Subject = "Reset temporary password";
            message.Body = new TextPart("plain")
            {
                Text = $"""
                Dear {user.Username},
                We hope this message finds you well. It has come to our attention that you currently 
                have a temporary password associated with your account.

                To reset your temporary password, please follow these simple steps:

                1. Click on the following link to access our password reset page: 
                ["https://localhost:7104/auth/resetpassword"].
                2. Enter your username and temporary password.
                3. Create new password and confirm it.

                Your username: {user.Username}
                Your temporary password: {user.Password}

                Best regards,

                InnoClinic
                Customer Support Team
                """
            };

            using var smtpClient = new SmtpClient();
            smtpClient.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            smtpClient.Authenticate(Environment.GetEnvironmentVariable("InnoClinic.Identity.Email"), Environment.GetEnvironmentVariable("InnoClinic.Identity.AppPassword"));
            await smtpClient.SendAsync(message);
            smtpClient.Disconnect(true);
        }
    }
}
