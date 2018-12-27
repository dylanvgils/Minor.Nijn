using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Minor.Nijn.RabbitMQBus;
using RabbitMQ.Client;
using System;

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

            services.TryAddSingleton(context);
            return services;
        }

        /// <summary>
        /// Adds and configures Minor.Nijn dependencies and returns a configures RabbitMQBusContext
        /// </summary>
        /// <returns>Configures RabbitMQBusContext</returns>
        public static IRabbitMQBusContext AddNijn(this IServiceCollection services, Action<IRabbitMQContextBuilder> action)
        {
            var builder = new RabbitMQContextBuilder(services);

            action(builder);
            builder.CreateContextAllowed = true;

            var context = builder.CreateContextWithRetry(Constants.RabbitMQRetryConnectTimes, Constants.RabbitMQRetryConnectTimeoutMs);
            services.AddNijn(context);

            return context;
        }
    }
}