using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn;
using Minor.Nijn.RabbitMQBus;
using System;
using System.Collections.Generic;
using System.Threading;

namespace RabbitMQ
{
    [TestClass]
    [Ignore]
    public class UnitTest1
    {
        [TestMethod]
        public void RabbitMQTest()
        {
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest")
                    .WithType("topic");

            string queue = "testqueue";
            List<string> topics = new List<string> { "topic1", "topic2" };

            using (IRabbitMQBusContext connection = connectionBuilder.CreateContext())
            {
                var receiver = connection.CreateMessageReceiver(queue, topics);
                receiver.DeclareQueue();
                var sender = connection.CreateMessageSender();

                bool callbackGingAf = false;

                EventMessageReceivedCallback e = new EventMessageReceivedCallback((EventMessage a) => callbackGingAf = true);
                receiver.StartReceivingMessages(e);

                sender.SendMessage(new EventMessage("topic1", "berichtje"));

                Thread.Sleep(5000);

                Assert.IsTrue(callbackGingAf);
            }
        }

        [TestMethod]
        public void CorrectMessageReceived()
        {
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest")
                    .WithType("topic");

            string queue = "testqueue";
            List<string> topics = new List<string> { "topic1", "topic2" };

            using (IRabbitMQBusContext connection = connectionBuilder.CreateContext())
            {
                var receiver = connection.CreateMessageReceiver(queue, topics);
                receiver.DeclareQueue();
                var sender = connection.CreateMessageSender();

                string msg = "";

                EventMessageReceivedCallback e = new EventMessageReceivedCallback((EventMessage a) => msg = a.Message);
                receiver.StartReceivingMessages(e);

                sender.SendMessage(new EventMessage("topic1", "berichtje"));

                Thread.Sleep(5000);

                Assert.AreEqual("berichtje", msg);
            }
        }

        [TestMethod]
        public void ReceiverListeningToCorrectTopics()
        {
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest")
                    .WithType("topic");

            string queue = "testqueue";
            List<string> topics = new List<string> { "topic1", "topic2" };

            using (IRabbitMQBusContext connection = connectionBuilder.CreateContext())
            {
                var receiver = connection.CreateMessageReceiver(queue, topics);
                receiver.DeclareQueue();
                var sender = connection.CreateMessageSender();

                string msg = "";

                EventMessageReceivedCallback e = new EventMessageReceivedCallback((EventMessage a) => msg = a.Message);
                receiver.StartReceivingMessages(e);

                sender.SendMessage(new EventMessage("topic1", "berichtTopic1"));
              
                Thread.Sleep(3000);

                Assert.AreEqual("berichtTopic1", msg);

                sender.SendMessage(new EventMessage("topic2", "BerichtTopic2"));

                Thread.Sleep(3000);

                Assert.AreEqual("BerichtTopic2", msg);
            }
        }
    }
}
