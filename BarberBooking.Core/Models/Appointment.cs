namespace BarberBooking.Core.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Status { get; set; } = "Booked"; // Booked, Completed, Canceled
        public bool IsPaid { get; set; } = false;
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CanceledAt { get; set; }

        // Navigation properties
        public Service? Service { get; set; }
    }
}
