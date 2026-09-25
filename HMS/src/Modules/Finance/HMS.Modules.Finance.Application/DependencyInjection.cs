using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Finance.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
