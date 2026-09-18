using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMS.Modules.Hotels.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddHotelsApplication(this IServiceCollection services)
        {
            services.AddMediatR(configuration =>
                configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

            return services;
        }
    }


}
