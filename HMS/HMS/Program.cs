using Scalar.AspNetCore;

namespace HMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                // OpenAPI specification
                app.MapOpenApi();

                // Interactive API documentation
                app.MapScalarApiReference(options =>
                {
                    options
                        .WithTitle("Fossyl Hotel Management API")
                        .WithTheme(ScalarTheme.DeepSpace);
                });
            }

            // HTTPS is handled by Docker ingress/Kong in production.
            // app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}