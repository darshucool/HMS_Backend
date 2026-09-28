using HMS.Modules.Payments.Application.Abstractions;
using HMS.Modules.Payments.Infrastructure.Persistence;
using HMS.Modules.Payments.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HMS.Modules.Payments.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IPaymentsDbConnectionFactory, PaymentsDbConnectionFactory>();
        services.AddScoped<IPaymentPropertyAccess, PaymentPropertyAccessRepository>();
        services.AddScoped<IPaymentBookingLookup, PaymentBookingLookup>();
        services.AddScoped<IBookingChargeRepository, BookingChargeRepository>();
        services.AddScoped<IBookingPaymentRepository, BookingPaymentRepository>();
        services.AddScoped<IBookingFinancialRepository, BookingFinancialRepository>();
        return services;
    }
}
