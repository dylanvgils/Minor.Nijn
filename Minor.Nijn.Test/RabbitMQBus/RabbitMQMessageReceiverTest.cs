using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System.Collections.Generic;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageReceiverTest
    {
        private string exchangeName = "exchangeName";
        private string queueName = "QueueName";
        private IEnumerable<string> topicExpressions = new List<string> { "a.b.c", "x.y.z" };
        
        private Mock<IRabbitMQBusContext> contextMock;
        private Mock<IModel> channelMock;
        
        private RabbitMQMessageReceiver target;
        
        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IRabbitMQBusContext>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);

            contextMock.Setup(ctx => ctx.Connection.CreateModel()).Returns(channelMock.Object);
            
            target = new RabbitMQMessageReceiver(contextMock.Object, queueName, topicExpressions);
        }

        [TestMethod]
        public void MessageReceiverIsCreatedWithCorrectParameters()
        {   
            contextMock.VerifyAll();
            
            Assert.IsNotNull(target);
            Assert.AreEqual(queueName, target.QueueName);
            Assert.AreEqual(topicExpressions, target.TopicExpressions);
            Assert.IsNotNull(target.Channel);    
        }
        
        [TestMethod]
        public void DeclareQueue_ShouldDeclareNewQueue()
        {
            contextMock.Setup(ctx => ctx.ExchangeName).Returns(exchangeName);
            channelMock.Setup(chan => chan.QueueDeclare(queueName, true, false, false, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            foreach (var topic in topicExpressions)
            {
                channelMock.Setup(chan => chan.QueueBind(queueName, exchangeName, topic, null));
            }

            target.DeclareQueue();

            contextMock.VerifyAll();
            channelMock.VerifyAll();
        }
        
        [TestMethod]
        public void StartReceivingMessages_ShouldStartListeningForMessages()
        {
            channelMock.Setup(chan =>
                    chan.BasicConsume(queueName, true, "", false, false, null, It.IsAny<EventingBasicConsumer>()))
                .Returns("Ok");
            
            EventMessage message = null;
            target.StartReceivingMessages(eventMessage => message = eventMessage);
            
            channelMock.VerifyAll();
        }
        
        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            channelMock.Setup(chan => chan.Dispose());
            target.Dispose();
            channelMock.VerifyAll();
        }
    }
}
