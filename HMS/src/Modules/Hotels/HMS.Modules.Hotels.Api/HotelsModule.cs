using HMS.Modules.Hotels.Application;
using HMS.Modules.Hotels.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Hotels.Api;

public static class HotelsModule
{
    public static IServiceCollection AddHotelsModule(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services.AddHotelsApplication();
        services.AddHotelsInfrastructure(configuration);

        return services;
    }
}

