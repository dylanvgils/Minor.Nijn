using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;

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
    }
}