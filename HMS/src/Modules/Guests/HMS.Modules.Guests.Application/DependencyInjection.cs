using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Guests.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddGuestsApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
