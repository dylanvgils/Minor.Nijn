using ConsoleAppExample.DAL;
using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.Helpers;
using Minor.Nijn.WebScale.Helpers;
using Serilog;
using System;
using System.Threading.Tasks;
using Minor.Nijn;
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

            // Create the service collection for the example console application
            var services = new ServiceCollection();
            services.AddTransient<IDataMapper<string, long>, SimpleDataMapper>();

            var busContext = services.AddNijn(options =>
            {
                options.SetLoggerFactory(loggerFactory);
                options.ReadFromEnvironmentVariables();
            });

            var hostBuilder = services.AddNijnWebScale(options =>
            {
                options.WithContext(busContext);
                options.SetLoggerFactory(loggerFactory);
                options.UseConventions();
                options.ScanForExceptions();
            });

            var serviceProvider = services.BuildServiceProvider();

            // Create instance of controller, with ICommandPublisher injected
            var controller = ActivatorUtilities.CreateInstance<Controller>(serviceProvider);

            // Create the microservice host and start listening
            using (var host = hostBuilder.CreateHost())
            {
                host.StartListening();

                await controller.SayHello("John");
                await controller.WhoopsExceptionThrown();
                await controller.WhoopsExternalExceptionThrown();
                await controller.AsyncCommandListenerMethod();
                controller.AsyncEventListenerMethod();
                controller.SpamEvents(10);

                Console.ReadKey();
            }
        }
    }
}
