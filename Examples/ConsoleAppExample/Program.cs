using ConsoleAppExample.DAL;
using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.Helpers;
using Minor.Nijn.RabbitMQBus;
using Minor.Nijn.WebScale;
using Minor.Nijn.WebScale.Helpers;
using Serilog;
using System;
using System.Threading.Tasks;
using LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory;

namespace ConsoleAppExample
{
    class Program
    {
        static void Main(string[] args)
        {
            new Program().Run();
        }

        private async Task Run()
        {
            // Create and configure logger factory
            var loggerFactory = new LoggerFactory()
                .AddSerilog(new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .CreateLogger()
                );

            ConsoleAppExampleLogger.LoggerFactory = loggerFactory;

            // Create a RabbitMQ context
            var busContext = new RabbitMQContextBuilder()
                .SetLoggerFactory(loggerFactory)
                .ReadFromEnvironmentVariables()
                .CreateContext();

            // Configure the microservice host
            var hostBuilder = new MicroserviceHostBuilder()
                .SetLoggerFactory(loggerFactory)
                .RegisterDependencies(nijnServices =>
                    {
                        nijnServices.AddTransient<IDataMapper<string, long>, SimpleDataMapper>();
                    })
                .WithContext(busContext)
                .UseConventions()
                .ScanForExceptions();

            // Create the service collection for the example console application
            var services = new ServiceCollection();
            services.AddNijn(busContext);
            services.AddNijnWebScale();

            // Create instance of controller, with ICommandPublisher injected
            var serviceProvider = services.BuildServiceProvider();
            var controller = ActivatorUtilities.CreateInstance<Controller>(serviceProvider);

            // Create the microservice host and start listening
            using (var host = hostBuilder.CreateHost())
            {
                host.RegisterListeners();

                await controller.SayHello("John");
                await controller.WhoopsExceptionThrown();
                await controller.WhoopsExternalExceptionThrown();
                await controller.AsyncCommandListenerMethod();
                controller.AsyncEventListenerMethod();

                Console.ReadKey();
            }
        }
    }
}
