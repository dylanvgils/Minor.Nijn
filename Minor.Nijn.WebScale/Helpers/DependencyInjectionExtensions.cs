using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using System;
using System.Reflection;

namespace Minor.Nijn.WebScale.Helpers
{
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds Minor.Nijn.WebScale dependencies to service collection
        /// </summary>
        public static IServiceCollection AddNijnWebScale(this IServiceCollection services)
        {
            services.AddTransient<IEventPublisher, EventPublisher>();
            services.AddTransient<ICommandPublisher, CommandPublisher>();
            return services;
        }

        /// <summary>
        /// Adds and configures the Minor.Nijn.WebScale dependencies and returns the configured MicroserviceHostBuilder
        /// </summary>
        /// <returns>Configured MicroServiceHostBuilder</returns>
        public static MicroserviceHostBuilder AddNijnWebScale(this IServiceCollection services, Action<IMicroserviceHostBuilder> action)
        {
            var assembly = Assembly.GetCallingAssembly();
            var builder = new MicroserviceHostBuilder(assembly, services);

            action(builder);
            builder.CreateHostAllowed = true;

            services.AddNijnWebScale();
            return builder;
        }
    }
}