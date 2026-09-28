using HMS.Modules.Booking.Application.Abstractions;
using HMS.Modules.Booking.Infrastructure.Persistence;
using HMS.Modules.Booking.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Booking.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IBookingDbConnectionFactory, BookingDbConnectionFactory>();
        services.AddScoped<IPropertyBookingAccess, PropertyBookingAccessRepository>();
        services.AddScoped<IBookingRepository, BookingRepository>();
        return services;
    }
}
