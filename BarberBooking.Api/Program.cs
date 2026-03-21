using Microsoft.EntityFrameworkCore;
using BarberBooking.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();

// Configurar DbContext com SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<BookingContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    // Criar database ao iniciar em dev
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BookingContext>();
        db.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
