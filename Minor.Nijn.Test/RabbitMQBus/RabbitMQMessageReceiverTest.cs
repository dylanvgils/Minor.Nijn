using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;
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
        private Mock<EventingBasicConsumerFactory> eventingBasicConsumerFactoryMock;
        
        private RabbitMQMessageReceiver target;
        
        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IRabbitMQBusContext>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);
            eventingBasicConsumerFactoryMock = new Mock<EventingBasicConsumerFactory>(MockBehavior.Strict);

            contextMock.Setup(ctx => ctx.Connection.CreateModel()).Returns(channelMock.Object);
            
            target = new RabbitMQMessageReceiver(contextMock.Object, queueName, topicExpressions, eventingBasicConsumerFactoryMock.Object);
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
            var consumer = new EventingBasicConsumer(channelMock.Object);

            var replyCommandMessage = "Reply message";
            var routingKey = "a.b.c";
            var type = "type";
            var timestamp = new AmqpTimestamp();
            var correlationId = "correlationId";
            
            var propsMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsMock.SetupGet(props => props.Type).Returns(type);
            propsMock.SetupGet(props => props.Timestamp).Returns(timestamp);
            propsMock.SetupGet(props => props.CorrelationId).Returns(correlationId);
            
            channelMock.Setup(chan =>
                    chan.BasicConsume(queueName, true, "", false, false, null, consumer))
                .Returns("Ok");
            
            eventingBasicConsumerFactoryMock.Setup(fact => fact.CreateEventingBasicConsumer(channelMock.Object))
                .Returns(consumer);
            
            EventMessage result = null;
            target.StartReceivingMessages(eventMessage => result = eventMessage);
            consumer.HandleBasicDeliver("", 1, false, "",  routingKey,  propsMock.Object, Encoding.UTF8.GetBytes(replyCommandMessage));
            
            channelMock.VerifyAll();
            contextMock.VerifyAll();
            eventingBasicConsumerFactoryMock.VerifyAll();
            propsMock.VerifyAll();
            
            Assert.AreEqual(result.Message, replyCommandMessage);
            Assert.AreEqual(result.RoutingKey, routingKey);
            Assert.AreEqual(result.Timestamp, timestamp.UnixTime);
            Assert.AreEqual(result.EventType, type);
            Assert.AreEqual(result.CorrelationId, correlationId);
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
