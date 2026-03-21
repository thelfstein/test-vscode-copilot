using Microsoft.EntityFrameworkCore;
using BarberBooking.Core.DTOs;
using BarberBooking.Core.Models;
using BarberBooking.Data;

namespace BarberBooking.Core.Services
{
    public interface IAppointmentService
    {
        Task<AppointmentDto?> GetByIdAsync(int id);
        Task<List<AppointmentDto>> GetByBarbeiroAsync(string barberId);
        Task<List<AppointmentDto>> GetByClienteAsync(string clientId);
        Task<AppointmentDto?> CreateAsync(string customerId, CreateAppointmentRequest request);
        Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentRequest request, string userId, string userType);
        Task<bool> CancelAsync(int id, string userId, string userType);
        Task<bool> MarkAsPaidAsync(int id, string userId, string userType);
        Task<List<AppointmentDto>> GetDisponibilidadeAsync(string barberId, DateTime data);
    }

    public class AppointmentService : IAppointmentService
    {
        private readonly BookingContext _context;
        private const int CANCELAMENTO_HORAS_ANTES = 24;

        public AppointmentService(BookingContext context)
        {
            _context = context;
        }

        public async Task<AppointmentDto?> GetByIdAsync(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            return ToDto(appointment);
        }

        public async Task<List<AppointmentDto>> GetByBarbeiroAsync(string barberId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.BarberId == barberId)
                .ToListAsync();

            return appointments.Select(ToDto).ToList();
        }

        public async Task<List<AppointmentDto>> GetByClienteAsync(string clientId)
        {
            var appointments = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.CustomerId == clientId)
                .ToListAsync();

            return appointments.Select(ToDto).ToList();
        }

        public async Task<AppointmentDto?> CreateAsync(string customerId, CreateAppointmentRequest request)
        {
            // Validar disponibilidade (não pode sobrepor)
            var tempoInicio = request.StartDateTime;
            
            var service = await _context.Services.FindAsync(request.ServiceId);
            if (service == null)
                return null;

            var tempoFinal = tempoInicio.AddMinutes(service.DurationMinutes);

            // Verificar overlap
            var overlap = await _context.Appointments
                .Where(a => a.BarberId == request.BarberId && a.Status == "Booked"
                    && a.StartDateTime < tempoFinal && a.EndDateTime > tempoInicio)
                .FirstOrDefaultAsync();

            if (overlap != null)
                return null; // Horário não disponível

            var appointment = new Appointment
            {
                CustomerId = customerId,
                BarberId = request.BarberId,
                ServiceId = request.ServiceId,
                StartDateTime = tempoInicio,
                EndDateTime = tempoFinal,
                Status = "Booked",
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            appointment.Service = service;
            return ToDto(appointment);
        }

        public async Task<AppointmentDto?> UpdateAsync(int id, UpdateAppointmentRequest request, string userId, string userType)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
                return null;

            // Apenas funcionário/admin pode alterar, ou cliente 24h antes
            if (userType == "Client" && (DateTime.UtcNow.AddHours(CANCELAMENTO_HORAS_ANTES) > appointment.StartDateTime || appointment.CustomerId != userId))
                return null; // Sem permissão

            if (request.BarberId != null)
                appointment.BarberId = request.BarberId;
            if (request.ServiceId.HasValue && request.ServiceId > 0)
            {
                var newService = await _context.Services.FindAsync(request.ServiceId);
                if (newService == null) return null;
                appointment.ServiceId = request.ServiceId.Value;
                appointment.Service = newService;
            }
            if (request.StartDateTime.HasValue)
                appointment.StartDateTime = request.StartDateTime.Value;

            if (appointment.Service != null)
                appointment.EndDateTime = appointment.StartDateTime.AddMinutes(appointment.Service.DurationMinutes);

            await _context.SaveChangesAsync();
            return ToDto(appointment);
        }

        public async Task<bool> CancelAsync(int id, string userId, string userType)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            // Cliente só pode cancelar até 24h antes
            if (userType == "Client" && appointment.CustomerId != userId)
                return false;

            if (userType == "Client" && DateTime.UtcNow.AddHours(CANCELAMENTO_HORAS_ANTES) > appointment.StartDateTime)
                return false; // Fora do prazo

            appointment.Status = "Canceled";
            appointment.CanceledAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsPaidAsync(int id, string userId, string userType)
        {
            if (userType == "Client")
                return false; // Cliente não pode marcar como pago

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return false;

            appointment.IsPaid = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<AppointmentDto>> GetDisponibilidadeAsync(string barberId, DateTime data)
        {
            // Blocos de 30/60 min - retorna slots ocupados
            var agendamentos = await _context.Appointments
                .Include(a => a.Service)
                .Where(a => a.BarberId == barberId 
                    && a.Status == "Booked"
                    && a.StartDateTime.Date == data.Date)
                .ToListAsync();

            return agendamentos.Select(ToDto).ToList();
        }

        private AppointmentDto ToDto(Appointment? appointment)
        {
            if (appointment == null) return null!;

            return new AppointmentDto
            {
                Id = appointment.Id,
                CustomerId = appointment.CustomerId,
                BarberId = appointment.BarberId,
                ServiceId = appointment.ServiceId,
                ServiceName = appointment.Service?.Name ?? "",
                StartDateTime = appointment.StartDateTime,
                EndDateTime = appointment.EndDateTime,
                Status = appointment.Status,
                IsPaid = appointment.IsPaid,
                Notes = appointment.Notes,
                CreatedAt = appointment.CreatedAt
            };
        }
    }
}
