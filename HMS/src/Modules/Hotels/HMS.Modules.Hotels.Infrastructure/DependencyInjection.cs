using HMS.Modules.Hotels.Application.Abstractions;
using HMS.Modules.Hotels.Infrastructure.Persistence;
using HMS.Modules.Hotels.Infrastructure.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Hotels.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHotelsInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSingleton<
                IHotelsDbConnectionFactory,
                HotelsDbConnectionFactory>();

            services.AddScoped<
                IOrganizationRepository,
                OrganizationRepository>();

            services.AddScoped<
                IPropertyRepository,
                PropertyRepository>();

            services.AddScoped<
                IAccommodationTypeRepository,
                AccommodationTypeRepository>();

            services.AddScoped<
                IAccommodationUnitRepository,
                AccommodationUnitRepository>();

            services.AddScoped<
                IUnitBlockRepository,
                UnitBlockRepository>();

            services.AddScoped<
                IPropertyAvailabilityRepository,
                PropertyAvailabilityRepository>();

            return services;
        }
    }
}
