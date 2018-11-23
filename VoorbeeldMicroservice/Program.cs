﻿using Minor.Nijn;
using Minor.Nijn.RabbitMQBus;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace VoorbeeldMicroservice
{
    public class Program
    {
        static void Main(string[] args)
        {
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest")
                    .WithType("topic");

            string queue = "testqueue";

            List<string> topics = new List<string> { "topic1", "topic2" };

            IMessageSender sender;
            IMessageReceiver receiver;

            using (IRabbitMQBusContext connection = connectionBuilder.CreateContext())
            {
                receiver = connection.CreateMessageReceiver(queue, topics);
                receiver.DeclareQueue();
                sender = connection.CreateMessageSender();
            
                string msg = "";

                EventMessageReceivedCallback e = new EventMessageReceivedCallback((EventMessage a) => msg = a.Message);
                receiver.StartReceivingMessages(e);
                
                sender.SendMessage(new EventMessage("topic1", "berichtje"));

                Console.ReadKey();
            }

            //using (RabbitMQBusContext connection = connectionBuilder.CreateContext())
            //{

            //    var builder = new MicroserviceHostBuilder()
            //            .WithConnection(connection)
            //            .RegisterDependencies((services) =>
            //                {
            //                    services.AddDbContext<PolisContext>(...);
            //                })
            //            .UseConventions();


            //    using (var host = builder.CreateHost())
            //    {
            //        host.Start();

            //        Console.WriteLine("ServiceHost is listening to incoming events...");
            //        Console.WriteLine("Press any key to quit.");
            //        Console.ReadKey();
            //    }
        }
        }
    }

