using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale
{
    public interface IMicroserviceHostBuilder
    {
        /// <summary>
        /// Configures the connection to the message broker
        /// </summary>
        MicroserviceHostBuilder WithContext(IBusContext<IConnection> context);

        /// <summary>
        /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
        /// </summary>
        MicroserviceHostBuilder UseConventions();

        /// <summary>
        /// Manually adds EventListeners to the MicroserviceHost
        /// </summary>
        MicroserviceHostBuilder AddListener<T>();

        /// <summary>
        /// Scans te calling assembly for exception types and adds them to the exception type dictionary
        /// </summary>
        MicroserviceHostBuilder ScanForExceptions();

        /// <summary>
        /// Scans the calling assembly for exceptions and exclude the given exclusions, adds the found
        /// exception type to the exception type dictionary
        /// </summary>
        /// <param name="exclusions">Assembly namespace prefixes to exclude</param>
        MicroserviceHostBuilder ScanForExceptions(IEnumerable<string> exclusions);

        /// <summary>
        /// Manually adds exception type to the exception type dictionary
        /// </summary>
        MicroserviceHostBuilder AddException<T>() where T : Exception;

        /// <summary>
        /// Configures logging functionality for the MicroserviceHost
        /// </summary>
        MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory);
    }
}