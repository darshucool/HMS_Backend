using HMS.Modules.Staff.Application.Abstractions;
using HMS.Modules.Staff.Infrastructure.Persistence;
using HMS.Modules.Staff.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Staff.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStaffInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IStaffDbConnectionFactory, StaffDbConnectionFactory>();
        services.AddScoped<IStaffPropertyAccess, StaffPropertyAccessRepository>();
        services.AddScoped<IStaffRepository, StaffRepository>();
        services.AddScoped<IStaffRoleRepository, StaffRoleRepository>();
        return services;
    }
}
