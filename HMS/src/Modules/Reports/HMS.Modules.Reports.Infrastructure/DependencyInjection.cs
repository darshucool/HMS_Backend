using HMS.Modules.Reports.Application.Abstractions;
using HMS.Modules.Reports.Infrastructure.Persistence;
using HMS.Modules.Reports.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Reports.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IReportsDbConnectionFactory, ReportsDbConnectionFactory>();
        services.AddScoped<IReportPropertyAccess, ReportPropertyAccessRepository>();
        services.AddScoped<IMonthlyPropertySummaryRepository, MonthlyPropertySummaryRepository>();
        services.AddScoped<IBookingRevenueRepository, BookingRevenueRepository>();
        services.AddScoped<IPropertyReportRepository, PropertyReportRepository>();
        return services;
    }
}
