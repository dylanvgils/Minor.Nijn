using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System.Collections.Generic;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageReceiver_Test
    {
        [TestMethod]
        public void QueueCanBeDeclared()
        {
            string queueName = "testQueue";
            string exchangeName = "testExchange";
            List<string> topicExpressions = new List<string> { "topic1", "topic1" };

            var channelMock = new Mock<IModel>(MockBehavior.Strict);
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);

            channelMock.Setup(c => c.QueueDeclare(queueName, false, false, false, null)).Returns(new QueueDeclareOk(queueName, 0, 0)).Verifiable();

            foreach (var topic in topicExpressions)
            {
                channelMock.Setup(c => c.QueueBind(queueName, exchangeName, topic, null)).Verifiable();
            }
            
            connectionMock.Setup(r => r.CreateModel())
                       .Returns(channelMock.Object)
                       .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, exchangeName);

            var target = new RabbitMQMessageReceiver(context, queueName, topicExpressions);

            target.DeclareQueue();

            channelMock.VerifyAll();
        }

        [TestMethod]
        public void MessageReceiverIsCreatedWithCorrectParameters()
        {
            var propsMock = new Mock<IBasicProperties>();
            var channelMock = new Mock<IModel>(MockBehavior.Strict);

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);

            connectionMock.Setup(r => r.CreateModel())
                       .Returns(channelMock.Object)
                       .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testxchange1");
            IEnumerable<string> topicExpressions = new List<string>() { "topic1", "topic2" };
            var target = new RabbitMQMessageReceiver(context, "Queue1", topicExpressions);

            Assert.IsNotNull(target);
            Assert.AreEqual("Queue1", target.QueueName);
            Assert.AreEqual(topicExpressions, target.TopicExpressions);
            Assert.IsNotNull(target.Channel);    
        }
    }
}
