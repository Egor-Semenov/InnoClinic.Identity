using InnoClinic.Identity.RabbitMQ.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace InnoClinic.Identity.RabbitMQ.Producers
{
    public sealed class PatientCreatedProducer : IMessageProducer
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public PatientCreatedProducer()
        {
            _factory = new ConnectionFactory { HostName = "localhost" };
            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        public void SendMessage<T>(T message)
        {
            var payload = JsonConvert.SerializeObject(message);
            var byteArray = Encoding.UTF8.GetBytes(payload);

            _channel.BasicPublish("user-profiles", "patient-created", null, byteArray);
        }
    }
}
