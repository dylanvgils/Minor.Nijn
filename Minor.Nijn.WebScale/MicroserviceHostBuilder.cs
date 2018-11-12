﻿using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.WebScale
{
    /// <summary>
    /// Creates and Configures a MicroserviceHost
    /// For example:
    ///     var builder = new MicroserviceHostBuilder()
    ///             .SetLoggerFactory(...)
    ///             .RegisterDependencies((services) =>
    ///                 {
    ///                     services.AddTransient<IFoo,Foo>();
    ///                 })
    ///             .WithBusOptions(new BusOptions(exchangeName: "MVM.TestExchange"))
    ///             .UseConventions();
    /// </summary>
    //public class MicroserviceHostBuilder
    //{
    //    /// <summary>
    //    /// Configures the connection to the message broker
    //    /// </summary>
    //    public MicroserviceHostBuilder WithContext(IBusContext context)
    //    {
    //    }

    //    /// <summary>
    //    /// Scans the assemblies for EventListeners and adds them to the MicroserviceHost
    //    /// </summary>
    //    public MicroserviceHostBuilder UseConventions()
    //    {
    //    }

    //    /// <summary>
    //    /// Manually adds EventListeners to the MicroserviceHost
    //    /// </summary>
    //    public MicroserviceHostBuilder AddEventListener<T>()
    //    {
    //    }

    //    /// <summary>
    //    /// Configures logging functionality for the MicroserviceHost
    //    /// </summary>
    //    public MicroserviceHostBuilder SetLoggerFactory(ILoggerFactory loggerFactory)
    //    {
    //    }

    //    /// <summary>
    //    /// Configures Dependency Injection for the MicroserviceHost
    //    /// </summary>
    //    public MicroserviceHostBuilder RegisterDependencies(Action<IServiceCollection> servicesConfiguration)
    //    {
    //    }

    //    /// <summary>
    //    /// Creates the MicroserviceHost, based on the configurations
    //    /// </summary>
    //    /// <returns></returns>
    //    public MicroserviceHost CreateHost()
    //    {
    //    }
    //}
}
