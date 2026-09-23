using HMS.Modules.Guests.Application;
using HMS.Modules.Guests.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Guests.Api;

public static class GuestsModule
{
    public static IServiceCollection AddGuestsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddGuestsApplication();
        services.AddGuestsInfrastructure(configuration);

        return services;
    }
}
