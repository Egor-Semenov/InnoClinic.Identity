using Azure.Identity;

namespace InnoClinic.Identity.RabbitMQ.Models.Receive
{
    public sealed record UserDeletedModel
    {
        public Guid UserId { get; set; }
    }
}
