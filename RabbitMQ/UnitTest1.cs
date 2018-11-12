using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn;
using Minor.Nijn.RabbitMQBus;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RabbitMQ
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest")
                    .WithType("topic");
                    

            string queue = "testqueue";
            List<string> topics = new List<string> { "topic1", "topic2" };

            using (RabbitMQBusContext connection = connectionBuilder.CreateContext())
            {
                var receiver = connection.CreateMessageReceiver(queue, topics);
                var sender = connection.CreateMessageSender();

                bool test = false;
                

                EventMessageReceivedCallback e = new EventMessageReceivedCallback((EventMessage a) => test = true);
                receiver.StartReceivingMessages(e);
                

                sender.SendMessage(new EventMessage("topic1", "berichtje"));

                Thread.Sleep(5000);

                Assert.IsTrue(test);

            }

            
        }
    }
}
