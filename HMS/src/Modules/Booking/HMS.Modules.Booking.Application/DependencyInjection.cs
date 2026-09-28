using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Booking.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBookingApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
