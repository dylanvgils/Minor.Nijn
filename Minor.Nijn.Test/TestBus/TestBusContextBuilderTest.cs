using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;

namespace Minor.Nijn.Test.TestBus
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
        public void IntegrationTest()
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
            
            Assert.AreEqual(result, message);
        }
    }
}
