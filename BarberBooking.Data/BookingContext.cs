using Microsoft.EntityFrameworkCore;
using BarberBooking.Core.Models;

namespace BarberBooking.Data
{
    public class BookingContext : DbContext
    {
        public BookingContext(DbContextOptions<BookingContext> options) : base(options) { }

        public DbSet<Service> Services { get; set; }
        public DbSet<ApplicationUser> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações de Service
            modelBuilder.Entity<Service>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.DurationMinutes).IsRequired();
            });

            // Configurações de ApplicationUser
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FullName).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.UserType).IsRequired().HasMaxLength(50);
            });

            // Configurações de Appointment
            modelBuilder.Entity<Appointment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CustomerId).IsRequired();
                entity.Property(e => e.BarberId).IsRequired();
                entity.Property(e => e.Status).IsRequired().HasMaxLength(50);
                entity.HasOne(e => e.Service)
                    .WithMany()
                    .HasForeignKey(e => e.ServiceId);
            });
        }
    }
}
