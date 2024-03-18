namespace InnoClinic.Identity.RabbitMQ.Models.Send
{
    public sealed class PatientCreatedModel
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }
    }
}
