using Xunit;
using Microsoft.EntityFrameworkCore;
using BarberBooking.Core.DTOs;
using BarberBooking.Core.Models;
using BarberBooking.Data.Services;
using BarberBooking.Data;

namespace BarberBooking.Tests
{
    public class AppointmentServiceTests : IDisposable
    {
        private readonly BookingContext _context;
        private readonly AppointmentService _appointmentService;

        public AppointmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<BookingContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new BookingContext(options);
            _appointmentService = new AppointmentService(_context);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var service = new Service
            {
                Id = 1,
                Name = "Corte Simples",
                DurationMinutes = 30,
                Price = 50,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Services.Add(service);
            _context.SaveChanges();
        }

        [Fact]
        public async Task CreateAsync_WithValidData_CreatesAppointment()
        {
            // Arrange
            var customerId = "customer-1";
            var barberId = "barber-1";
            var request = new CreateAppointmentRequest
            {
                BarberId = barberId,
                ServiceId = 1,
                StartDateTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10)
            };

            // Act
            var result = await _appointmentService.CreateAsync(customerId, request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(customerId, result.CustomerId);
            Assert.Equal(barberId, result.BarberId);
            Assert.Equal("Booked", result.Status);
            Assert.False(result.IsPaid);
        }

        [Fact]
        public async Task CreateAsync_WithOverlappingTime_ReturnNull()
        {
            // Arrange
            var customerId = "customer-1";
            var barberId = "barber-1";
            var now = DateTime.UtcNow.AddDays(1).Date.AddHours(10);

            // Criar primeiro agendamento
            var appointment1 = new Appointment
            {
                CustomerId = customerId,
                BarberId = barberId,
                ServiceId = 1,
                StartDateTime = now,
                EndDateTime = now.AddMinutes(30),
                Status = "Booked",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment1);
            _context.SaveChanges();

            // Tentar sobrepor
            var request = new CreateAppointmentRequest
            {
                BarberId = barberId,
                ServiceId = 1,
                StartDateTime = now.AddMinutes(15) // Sobrepõe com o primeiro
            };

            // Act
            var result = await _appointmentService.CreateAsync(customerId, request);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task CancelAsync_WithinTimeWindow_ReturnTrue()
        {
            // Arrange
            var customerId = "customer-1";
            var appointmentTime = DateTime.UtcNow.AddHours(25); // 25 horas no futuro

            var appointment = new Appointment
            {
                CustomerId = customerId,
                BarberId = "barber-1",
                ServiceId = 1,
                StartDateTime = appointmentTime,
                EndDateTime = appointmentTime.AddMinutes(30),
                Status = "Booked",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            // Act
            var result = await _appointmentService.CancelAsync(appointment.Id, customerId, "Client");

            // Assert
            Assert.True(result);
            var cancelledAppointment = _context.Appointments.Find(appointment.Id);
            Assert.Equal("Canceled", cancelledAppointment?.Status);
            Assert.NotNull(cancelledAppointment?.CanceledAt);
        }

        [Fact]
        public async Task CancelAsync_WithinLessThan24Hours_ReturnFalse()
        {
            // Arrange
            var customerId = "customer-1";
            var appointmentTime = DateTime.UtcNow.AddHours(23); // 23 horas no futuro

            var appointment = new Appointment
            {
                CustomerId = customerId,
                BarberId = "barber-1",
                ServiceId = 1,
                StartDateTime = appointmentTime,
                EndDateTime = appointmentTime.AddMinutes(30),
                Status = "Booked",
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            // Act
            var result = await _appointmentService.CancelAsync(appointment.Id, customerId, "Client");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MarkAsPaidAsync_ByEmployee_ReturnTrue()
        {
            // Arrange
            var appointment = new Appointment
            {
                CustomerId = "customer-1",
                BarberId = "barber-1",
                ServiceId = 1,
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
                Status = "Booked",
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            // Act
            var result = await _appointmentService.MarkAsPaidAsync(appointment.Id, "barber-1", "Employee");

            // Assert
            Assert.True(result);
            var updatedAppointment = _context.Appointments.Find(appointment.Id);
            Assert.True(updatedAppointment?.IsPaid);
        }

        [Fact]
        public async Task MarkAsPaidAsync_ByClient_ReturnFalse()
        {
            // Arrange
            var appointment = new Appointment
            {
                CustomerId = "customer-1",
                BarberId = "barber-1",
                ServiceId = 1,
                StartDateTime = DateTime.UtcNow.AddDays(1),
                EndDateTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
                Status = "Booked",
                IsPaid = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Appointments.Add(appointment);
            _context.SaveChanges();

            // Act
            var result = await _appointmentService.MarkAsPaidAsync(appointment.Id, "customer-1", "Client");

            // Assert
            Assert.False(result);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
