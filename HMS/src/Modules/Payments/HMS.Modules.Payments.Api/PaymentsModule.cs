using HMS.Modules.Payments.Application;
using HMS.Modules.Payments.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Payments.Api;

public static class PaymentsModule
{
    public static IServiceCollection AddPaymentsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddPaymentsApplication();
        services.AddPaymentsInfrastructure(configuration);
        return services;
    }
}
