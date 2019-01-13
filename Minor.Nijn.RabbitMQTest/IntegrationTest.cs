using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn;
using Minor.Nijn.RabbitMQBus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.Helpers;
using RabbitMQ.Client;
using Serilog.Core;

namespace RabbitMQ
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void CorrectMessageReceived()
        {
            var queue = "testQueue";
            var topics = new List<string> { "topic1", "topic2" };
            var message = new EventMessage("topic1", "Message 1", "string", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "correlationId");

            var connectionBuilder = new RabbitMQContextBuilder()
                .WithExchange("MVM.EventExchange")
                .WithAddress("localhost", 5672)
                .WithCredentials(userName: "guest", password: "guest")
                .WithType("topic");

            using (IRabbitMQBusContext connection = connectionBuilder.CreateContext())
            {
                var receiver = connection.CreateMessageReceiver(queue, topics);
                receiver.DeclareQueue();

                var sender = connection.CreateMessageSender();

                var flag = new ManualResetEvent(false);
                EventMessage result = null;
                receiver.StartReceivingMessages(msg =>
                {
                    result = msg;
                    flag.Set();
                });

                sender.SendMessage(message);
                flag.WaitOne(5000);

                Assert.AreEqual(message.RoutingKey, result.RoutingKey);
                Assert.AreEqual(message.CorrelationId, result.CorrelationId);
                Assert.AreEqual(message.Type, result.Type);
                Assert.AreEqual(message.Timestamp, result.Timestamp);
                Assert.AreEqual(message.Message, result.Message);
            }
        }

        [TestMethod]
        public void ReceiverListeningToCorrectTopics()
        {
            var queue = "testQueue";
            var topics = new List<string> { "topic1", "topic2" };
            var message1 = new EventMessage("topic1", "Message 1", "string", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "correlationId");
            var message2 = new EventMessage("topic2", "Message 2", "string", DateTimeOffset.Now.ToUnixTimeMilliseconds(), "anotherCorrelationId");

            var connectionBuilder = new RabbitMQContextBuilder()
                .WithExchange("MVM.EventExchange")
                .WithAddress("localhost", 5672)
                .WithCredentials(userName: "guest", password: "guest")
                .WithType("topic");

            using (IRabbitMQBusContext connection = connectionBuilder.CreateContext())
            {
                var receiver = connection.CreateMessageReceiver(queue, topics);
                receiver.DeclareQueue();

                var sender = connection.CreateMessageSender();

                var flag = new ManualResetEvent(false);
                EventMessage result = null;
                receiver.StartReceivingMessages(msg =>
                {
                    result = msg;
                    flag.Set();
                });

                sender.SendMessage(message1);
                flag.WaitOne(5000);

                Assert.AreEqual(message1.RoutingKey, result.RoutingKey);
                Assert.AreEqual(message1.CorrelationId, result.CorrelationId);
                Assert.AreEqual(message1.Type, result.Type);
                Assert.AreEqual(message1.Timestamp, result.Timestamp);
                Assert.AreEqual(message1.Message, result.Message);

                flag.Reset();
                sender.SendMessage(message2);
                flag.WaitOne(5000);

                Assert.AreEqual(message2.RoutingKey, result.RoutingKey);
                Assert.AreEqual(message2.CorrelationId, result.CorrelationId);
                Assert.AreEqual(message2.Type, result.Type);
                Assert.AreEqual(message2.Timestamp, result.Timestamp);
                Assert.AreEqual(message2.Message, result.Message);
            }
        }

        [TestMethod]
        public async Task CommandCanBeSentAndReceived()
        {
            var queueName = "TestCommandQueue";
            var requestCommand = new RequestCommandMessage("Request message", "type", "correlationId",  queueName, DateTimeOffset.Now.ToUnixTimeMilliseconds());
            var responseCommand = new ResponseCommandMessage("Reply message", "type", requestCommand.CorrelationId, DateTimeOffset.Now.ToUnixTimeMilliseconds());
            
            var connectionBuilder = new RabbitMQContextBuilder()
                .WithExchange("MVM.EventExchange")
                .WithAddress("localhost", 5672)
                .WithCredentials(userName: "guest", password: "guest")
                .WithType("topic");

            using (IRabbitMQBusContext context = connectionBuilder.CreateContext())
            {
                var receiver = context.CreateCommandReceiver(queueName);
                receiver.DeclareCommandQueue();

                RequestCommandMessage request = null;
                receiver.StartReceivingCommands(req =>
                {
                    request = req;
                    return responseCommand;
                });

                var sender = context.CreateCommandSender();
                var response = await sender.SendCommandAsync(requestCommand);

                Assert.AreEqual(requestCommand.CorrelationId, request.CorrelationId);
                Assert.AreEqual(requestCommand.Timestamp, request.Timestamp);
                Assert.AreEqual(requestCommand.Type, request.Type);
                Assert.AreEqual(requestCommand.Message, request.Message);

                Assert.IsNull(responseCommand.RoutingKey);
                Assert.AreEqual(responseCommand.CorrelationId, response.CorrelationId);
                Assert.AreEqual(responseCommand.Timestamp, response.Timestamp);
                Assert.AreEqual(responseCommand.Type, response.Type);
                Assert.AreEqual(responseCommand.Message, response.Message);
            }
        }

        [TestMethod]
        public void CanBeIntegratedWithTheApplicationDependencyServiceCollection()
        {
            var services = new ServiceCollection();

            Environment.SetEnvironmentVariable("NIJN_EXCHANGE_NAME", "MVM.EventExchange");
            Environment.SetEnvironmentVariable("NIJN_HOSTNAME", "localhost");
            Environment.SetEnvironmentVariable("NIJN_PORT", "5672");
            Environment.SetEnvironmentVariable("NIJN_USERNAME", "guest");
            Environment.SetEnvironmentVariable("NIJN_PASSWORD", "guest");

            using (var context = services.AddNijn(options => { options.ReadFromEnvironmentVariables(); }))
            {
                Assert.IsTrue(services.Any(s => s.ServiceType == typeof(IBusContext<IConnection>)));
            }
        }
    }
}
