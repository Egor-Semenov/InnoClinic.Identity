using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using InnoClinic.Identity.RabbitMQ.Models.Receive;
using InnoClinic.Identity.Models.Entities;
using Microsoft.AspNetCore.Identity;

namespace InnoClinic.Identity.RabbitMQ.Subscribers
{
    public sealed class UserDeletedSubscriber : BackgroundService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _scopeFactory;

        private const string Queue = "identity-service/user-deleted";

        public UserDeletedSubscriber(IServiceScopeFactory scopeFactory)
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
                var message = JsonConvert.DeserializeObject<UserDeletedModel>(contentString);

                using var scope = _scopeFactory.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

                var user = await userManager.FindByIdAsync(message.UserId.ToString()); 

                var result = await userManager.DeleteAsync(user!);
                if (!result.Succeeded)
                {
                    _channel.BasicNack(eventArgs.DeliveryTag, false, false);
                }

                _channel.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume(Queue, false, consumer);

            return Task.CompletedTask;
        }
    }
}
