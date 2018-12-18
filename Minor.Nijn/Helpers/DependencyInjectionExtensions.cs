using System;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Minor.Nijn.Helpers
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds Minor.Nijn dependencies to the service collection
        /// </summary>
        public static IServiceCollection AddNijn(this IServiceCollection services, IBusContext<IConnection> context)
        {
            if (context == null)
            {
                throw new ArgumentException("Context can not be null");
            }

            services.AddSingleton(context);
            return services;
        }
    }
}