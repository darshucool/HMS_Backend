using HMS.Modules.Booking.Application;
using HMS.Modules.Booking.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Booking.Api;

public static class BookingModule
{
    public static IServiceCollection AddBookingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddBookingApplication();
        services.AddBookingInfrastructure(configuration);
        return services;
    }
}
