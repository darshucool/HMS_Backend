using System.Text;
using HMS.Modules.Identity.Application.Abstractions;
using HMS.Modules.Identity.Infrastructure.Authentication;
using HMS.Modules.Identity.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace HMS.Modules.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("HmsDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'HmsDatabase' is missing.");

        var jwtSection = configuration.GetSection("Jwt");

        var issuer = jwtSection["Issuer"]
            ?? throw new InvalidOperationException(
                "JWT Issuer is missing.");

        var audience = jwtSection["Audience"]
            ?? throw new InvalidOperationException(
                "JWT Audience is missing.");

        var secretKey = jwtSection["SecretKey"]
            ?? throw new InvalidOperationException(
                "JWT SecretKey is missing.");

        if (secretKey.Length < 32)
        {
            throw new InvalidOperationException(
                "JWT SecretKey must contain at least 32 characters.");
        }

        services.AddSingleton(
            NpgsqlDataSource.Create(connectionString));

        services.AddAuthentication(
                JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,

                        ValidateAudience = true,
                        ValidAudience = audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(secretKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromSeconds(30)
                    };
            });

        services.Configure<JwtOptions>(jwtSection);

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                "PlatformAdmin",
                policy => policy.RequireRole("PLATFORM_ADMIN"));
        });

        services.AddSingleton(TimeProvider.System);

        services.AddScoped<
            IStaffRepository,
            StaffRepository>();

        services.AddScoped<
            IStaffTokenService,
            StaffTokenService>();

        services.AddScoped<
            IPasswordHasher<StaffLoginRecord>,
            PasswordHasher<StaffLoginRecord>>();

        return services;
    }
}