using Microsoft.AspNetCore.Identity;
using BarberBooking.Core.Models;

namespace BarberBooking.Data.Services
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }

    public class DbInitializer : IDbInitializer
    {
        private readonly BookingContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DbInitializer(BookingContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task InitializeAsync()
        {
            // Criar usuário admin padrão
            var adminEmail = "admin@barberbooking.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin Barbearia",
                    UserType = "Admin",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123456");
                if (!result.Succeeded)
                {
                    throw new Exception($"Erro ao criar usuário admin: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Criar serviços padrão
            if (!_context.Services.Any())
            {
                var servicos = new[]
                {
                    new Service
                    {
                        Name = "Corte Simples",
                        DurationMinutes = 30,
                        Price = 50.00m,
                        Active = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Service
                    {
                        Name = "Corte + Barba",
                        DurationMinutes = 60,
                        Price = 80.00m,
                        Active = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Service
                    {
                        Name = "Barba Técnica",
                        DurationMinutes = 30,
                        Price = 40.00m,
                        Active = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Service
                    {
                        Name = "Corte Degradê",
                        DurationMinutes = 45,
                        Price = 65.00m,
                        Active = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                _context.Services.AddRange(servicos);
                await _context.SaveChangesAsync();
            }

            // Criar barbeiros padrão (funcionários)
            var funcionarioEmail = "barber@barberbooking.com";
            var barberUser = await _userManager.FindByEmailAsync(funcionarioEmail);

            if (barberUser == null)
            {
                barberUser = new ApplicationUser
                {
                    UserName = funcionarioEmail,
                    Email = funcionarioEmail,
                    FullName = "João Barbeiro",
                    UserType = "Employee",
                    PhoneNumber = "(11) 99999-9999",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(barberUser, "Barber@123456");
                if (!result.Succeeded)
                {
                    throw new Exception($"Erro ao criar usuário barbeiro: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            // Outro barbeiro
            var barber2Email = "barber2@barberbooking.com";
            var barber2User = await _userManager.FindByEmailAsync(barber2Email);

            if (barber2User == null)
            {
                barber2User = new ApplicationUser
                {
                    UserName = barber2Email,
                    Email = barber2Email,
                    FullName = "Carlos Barbeiro",
                    UserType = "Employee",
                    PhoneNumber = "(11) 98888-8888",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(barber2User, "Barber@123456");
                if (!result.Succeeded)
                {
                    throw new Exception($"Erro ao criar usuário barbeiro2: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
