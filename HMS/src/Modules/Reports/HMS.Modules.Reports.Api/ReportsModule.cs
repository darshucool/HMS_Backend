using HMS.Modules.Reports.Application;
using HMS.Modules.Reports.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Reports.Api;

public static class ReportsModule
{
    public static IServiceCollection AddReportsModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddReportsApplication();
        services.AddReportsInfrastructure(configuration);
        return services;
    }
}
