using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarberBooking.Core.DTOs;
using BarberBooking.Data.Services;

namespace BarberBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var appointment = await _appointmentService.GetByIdAsync(id);
            if (appointment == null)
                return NotFound();

            return Ok(appointment);
        }

        [HttpGet("barber/{barberId}")]
        [Authorize(Policy = "EmployeeAdminOnly")]
        public async Task<IActionResult> GetByBarber(string barberId)
        {
            var appointments = await _appointmentService.GetByBarbeiroAsync(barberId);
            return Ok(appointments);
        }

        [HttpGet("my-appointments")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var appointments = await _appointmentService.GetByClienteAsync(userId);
            return Ok(appointments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appointment = await _appointmentService.CreateAsync(userId, request);
            if (appointment == null)
                return BadRequest(new { message = "Horário não disponível ou serviço não encontrado" });

            return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateAppointmentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return Unauthorized();

            var appointment = await _appointmentService.UpdateAsync(id, request, userId, userType);
            if (appointment == null)
                return BadRequest(new { message = "Atualização não permitida ou agendamento não encontrado" });

            return Ok(appointment);
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return Unauthorized();

            var result = await _appointmentService.CancelAsync(id, userId, userType);
            if (!result)
                return BadRequest(new { message = "Cancelamento não permitido (fora do prazo ou sem permissão)" });

            return NoContent();
        }

        [HttpPost("{id}/pay")]
        [Authorize(Policy = "EmployeeAdminOnly")]
        public async Task<IActionResult> MarkAsPaid(int id, [FromBody] PayAppointmentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userType = User.FindFirst("UserType")?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(userType))
                return Unauthorized();

            var result = await _appointmentService.MarkAsPaidAsync(id, userId, userType);
            if (!result)
                return BadRequest(new { message = "Erro ao marcar como pago" });

            return NoContent();
        }

        [HttpGet("disponibilidade/{barberId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetDisponibilidade(string barberId, [FromQuery] DateTime data)
        {
            var appointments = await _appointmentService.GetDisponibilidadeAsync(barberId, data);
            return Ok(appointments);
        }
    }
}
