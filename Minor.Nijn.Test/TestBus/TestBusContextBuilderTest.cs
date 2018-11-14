using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.TestBus.CommandBus;

namespace Minor.Nijn.TestBus.Test
{
    [TestClass]
    public class TestBusContextBuilderTest
    {
        [TestMethod]
        public void CreateContext_ShouldReturnNewTestBusContextInstance()
        {
            var target = TestBusContextBuilder.CreateContext();
            Assert.IsInstanceOfType(target, typeof(TestBusContext));
        }

        [TestMethod]
        public void CreateContext_ShouldReturnSameInstanceWhenCalledTheSecondTime()
        {
            var context1 = TestBusContextBuilder.CreateContext();
            var context2 = TestBusContextBuilder.CreateContext();

            Assert.AreEqual(context1, context2);
        }
        
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
        public void IntegrationTestCommand()
        {
            IBusContextExtension taraget = TestBusContextBuilder.CreateContext();
            
            var response = new CommandMessage("Reply message", "type", "id");
            var request = new CommandMessage("Test message", "type", "id");
            request.ReplyTo = "ReplyQueue1";

            var sender = taraget.CreateTestCommandSender();
            sender.ReplyMessage = response;
            
            var result = sender.SendCommandAsync(request);
            Console.WriteLine(result.Result);
            
            Assert.AreEqual(response,  result.Result);
        }
    }
}
