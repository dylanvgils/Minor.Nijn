using Minor.Nijn;
using Minor.Nijn.RabbitMQBus;
using Minor.Nijn.WebScale;
using RabbitMQ.Client;
using System;

namespace VoorbeeldMicroservice
{
    public class Program
    {
        static void Main(string[] args)
        {/*
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest")
                    .ReadFromEnvironmentVariables();    // beetje dubbel-op, misschien


            using (IBusContext connection = connectionBuilder.CreateConnection())
            {

                var builder = new MicroserviceHostBuilder()
                        .WithConnection(connection)
                        .RegisterDependencies((services) =>
                            {
                                services.AddDbContext<PolisContext>(...);
                            })
                        .UseConventions();


                using (var host = builder.CreateHost())
                {
                    host.Start();

                    Console.WriteLine("ServiceHost is listening to incoming events...");
                    Console.WriteLine("Press any key to quit.");
                    Console.ReadKey();
                }
            }
        */}
    }
}
