namespace BarberBooking.Core.DTOs
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public bool Active { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateServiceRequest
    {
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
    }

    public class UpdateServiceRequest
    {
        public string? Name { get; set; }
        public int? DurationMinutes { get; set; }
        public decimal? Price { get; set; }
        public bool? Active { get; set; }
    }
}
