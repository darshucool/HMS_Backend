using HMS.Modules.Finance.Application;
using HMS.Modules.Finance.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Finance.Api;

public static class FinanceModule
{
    public static IServiceCollection AddFinanceModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddFinanceApplication();
        services.AddFinanceInfrastructure(configuration);
        return services;
    }
}
