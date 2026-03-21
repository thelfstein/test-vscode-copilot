namespace BarberBooking.Core.DTOs
{
    public class AppointmentDto
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string BarberId { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsPaid { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateAppointmentRequest
    {
        public string BarberId { get; set; } = string.Empty;
        public int ServiceId { get; set; }
        public DateTime StartDateTime { get; set; }
    }

    public class UpdateAppointmentRequest
    {
        public string? BarberId { get; set; }
        public int? ServiceId { get; set; }
        public DateTime? StartDateTime { get; set; }
    }

    public class PayAppointmentRequest
    {
        public bool IsPaid { get; set; }
    }
}
