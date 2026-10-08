using HMS.Modules.Staff.Application;
using HMS.Modules.Staff.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Staff.Api;

public static class StaffModule
{
    public static IServiceCollection AddStaffModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddStaffApplication();
        services.AddStaffInfrastructure(configuration);
        return services;
    }
}
