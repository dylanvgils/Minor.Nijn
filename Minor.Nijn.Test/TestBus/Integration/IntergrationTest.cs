using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus.CommandBus;
using System.Collections.Generic;

namespace Minor.Nijn.TestBus.Integration.Test
{
    [TestClass]
    public class IntegrationTest
    {
        [TestMethod]
        public void EventSendWithSenderIsReceivedInReceiver()
        {
            var target = new TestBusContextBuilder().CreateTestContext();

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
        public void CommandIsSentToTheRightQueueAndReturnsRightResponse()
        {
            var queueName = "CommandQueue";
            var target = new TestBusContextBuilder().CreateTestContext();
            target.CommandBus.DeclareCommandQueue(queueName);

            var request = new RequestCommandMessage("Test message", "type", "id", queueName);

            var sender = (ITestCommandSender)target.CreateCommandSender();
            var result = sender.SendCommandAsync(request);

            var response = new ResponseCommandMessage("Reply message", "type", "id") { RoutingKey = sender.ReplyQueueName };
            var responseBusCommand = new TestBusCommand("ReplyTo", response);
            target.CommandBus.Queues[sender.ReplyQueueName].Enqueue(responseBusCommand);

            Assert.AreEqual(2, target.CommandBus.QueueCount);
            Assert.AreEqual(1, target.CommandBus.Queues[queueName].MessageQueueLength);
            Assert.AreEqual(response, result.Result);
        }

        [TestMethod]
        public void ReceiverReceivesCommandSendToQueue()
        {
            var queueName = "CommandQueue";
            var target = new TestBusContextBuilder().CreateTestContext();

            var receiver = target.CreateCommandReceiver(queueName);
            receiver.DeclareCommandQueue();
            var commandMessage = new RequestCommandMessage("Reply message", "type", "id", queueName);
            var command = new TestBusCommand(null, commandMessage);

            CommandMessage result = null;
            receiver.StartReceivingCommands(c =>
            {
                result = c;
                return null;
            });
            target.CommandBus.DispatchMessage(command);

            Assert.AreEqual(commandMessage, result);
        }
    }
}
