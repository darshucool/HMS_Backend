using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Reports.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
