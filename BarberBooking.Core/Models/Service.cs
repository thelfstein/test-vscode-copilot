namespace BarberBooking.Core.Models
{
    public class Service
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int DurationMinutes { get; set; }
        public decimal Price { get; set; }
        public string? CreatedByUserId { get; set; }
        public bool Active { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
