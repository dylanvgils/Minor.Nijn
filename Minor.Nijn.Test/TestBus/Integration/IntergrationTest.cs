using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Minor.Nijn.TestBus.Integration.Test
{
    [TestClass]
    public class IntergrationTest
    {
        [TestMethod]
        public void IntegrationTestEvent()
        {
            var target = TestBusContextBuilder.CreateContext();

            string routingKey = "a.b.c";
            string queueName = "TestQueue";
            IEnumerable<string> topicExpressions = new List<string> { routingKey };

            var sender = target.CreateMessageSender();
            var receiver = target.CreateMessageReceiver(queueName, topicExpressions);
            var message = new EventMessage(routingKey, "Test message");

            EventMessage result = null;
            receiver.DeclareQueue();
            receiver.StartReceivingMessages(eventMessage => result = eventMessage);
            sender.SendMessage(message);

            Assert.AreEqual(message, result);
        }

        [TestMethod]
        public void IntegrationTestCommandSend()
        {
            IBusContextExtension taraget = TestBusContextBuilder.CreateContext();

            var response = new CommandMessage("Reply message", "type", "id");
            var request = new CommandMessage("Test message", "type", "id")
            {
                ReplyTo = "ReplyQueue1"
            };

            var sender = taraget.CreateMockCommandSender();
            sender.ReplyMessage = response;
            var result = sender.SendCommandAsync(request);

            Assert.AreEqual(response, result.Result);
        }

        [TestMethod]
        public void IntegrationTestCommandReceive()
        {
            IBusContextExtension taraget = TestBusContextBuilder.CreateContext();

            var receiver = taraget.CreateCommandReceiver();
            receiver.DeclareCommandQueue();
            var command = new CommandMessage("Reply message", "type", "id");

            CommandMessage result = null;
            receiver.StartReceivingCommands(c => result = c);
            taraget.SendMockCommand(command);

            Assert.AreEqual(command, result);
        }
    }
}
