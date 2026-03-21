using Swashbuckle.AspNetCore.SwaggerGen;

namespace BarberBooking.Api.Swagger
{
    public static class SwaggerConfigurationExtensions
    {
        public static void AddSwaggerAuthentication(this SwaggerGenOptions options)
        {
            // Configuração simples - o Swagger UI padrão permite adicionar headers
            // via interface de "Authorize" intuitiva
        }
    }
}



