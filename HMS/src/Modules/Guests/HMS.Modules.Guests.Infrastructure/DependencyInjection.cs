using HMS.Modules.Guests.Application.Abstractions;
using HMS.Modules.Guests.Infrastructure.Persistence;
using HMS.Modules.Guests.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Guests.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddGuestsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IGuestsDbConnectionFactory, GuestsDbConnectionFactory>();
        services.AddScoped<IPropertyAccess, PropertyAccessRepository>();
        services.AddScoped<IGuestRepository, GuestRepository>();
        services.AddScoped<IGuestDocumentRepository, GuestDocumentRepository>();
        services.AddScoped<IGuestPreferenceRepository, GuestPreferenceRepository>();

        return services;
    }
}
