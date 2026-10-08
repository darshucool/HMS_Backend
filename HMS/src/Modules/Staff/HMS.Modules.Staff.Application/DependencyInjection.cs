using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Staff.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddStaffApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
