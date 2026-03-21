using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BarberBooking.Core.DTOs;
using BarberBooking.Core.Models;
using BarberBooking.Data;

namespace BarberBooking.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServicesController : ControllerBase
    {
        private readonly BookingContext _context;

        public ServicesController(BookingContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var services = await Task.FromResult(_context.Services
                .Where(s => s.Active)
                .Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    DurationMinutes = s.DurationMinutes,
                    Price = s.Price,
                    Active = s.Active,
                    CreatedAt = s.CreatedAt
                })
                .ToList());

            return Ok(services);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == id);
            if (service == null)
                return NotFound();

            return Ok(new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                Active = service.Active,
                CreatedAt = service.CreatedAt
            });
        }

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateServiceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var service = new Service
            {
                Name = request.Name,
                DurationMinutes = request.DurationMinutes,
                Price = request.Price,
                Active = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = service.Id }, new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                Active = service.Active,
                CreatedAt = service.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceRequest request)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == id);
            if (service == null)
                return NotFound();

            if (!string.IsNullOrEmpty(request.Name))
                service.Name = request.Name;
            if (request.DurationMinutes.HasValue)
                service.DurationMinutes = request.DurationMinutes.Value;
            if (request.Price.HasValue)
                service.Price = request.Price.Value;
            if (request.Active.HasValue)
                service.Active = request.Active.Value;

            await _context.SaveChangesAsync();

            return Ok(new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                DurationMinutes = service.DurationMinutes,
                Price = service.Price,
                Active = service.Active,
                CreatedAt = service.CreatedAt
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = _context.Services.FirstOrDefault(s => s.Id == id);
            if (service == null)
                return NotFound();

            // Soft delete
            service.Active = false;
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
