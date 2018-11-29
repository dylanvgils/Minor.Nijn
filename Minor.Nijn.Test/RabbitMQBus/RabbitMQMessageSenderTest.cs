using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageSenderTest
    {
        private string exchangeName = "exchangeName";
        
        private Mock<IRabbitMQBusContext> contextMock;
        private Mock<IModel> channelMock;
        
        private RabbitMQMessageSender target;
        
        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IRabbitMQBusContext>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);

            contextMock.Setup(ctx => ctx.Connection.CreateModel()).Returns(channelMock.Object);
            
            target = new RabbitMQMessageSender(contextMock.Object);
        }

        [TestMethod]
        public void MessageSenderIsCreatedWithCorrectParameters()
        {
            contextMock.VerifyAll();

            Assert.IsNotNull(target);
            Assert.IsNotNull(target.Channel);
        }

        [TestMethod]
        public void SendMessage_ShouldCallBasicPublishWithCorrectMessage()
        {
            var type = "type";
            var correlationId = "correlationId";
            var routingKey = "routingKey";
            var timestamp = DateTime.Now.Ticks;
            var messageBody = "Test message";
            
            var message = new EventMessage(
                routingKey: routingKey,
                message: messageBody,
                type: type,
                timestamp: timestamp,
                correlationId: correlationId
            );
            
            var propsMock = new Mock<BasicProperties>(MockBehavior.Strict);
            propsMock.SetupSet(props => props.Type = type);
            propsMock.SetupSet(props => props.CorrelationId = correlationId);
            propsMock.SetupSet(props => props.Timestamp = new AmqpTimestamp(timestamp));

            contextMock.Setup(ctx => ctx.ExchangeName).Returns(exchangeName);
            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsMock.Object);
            channelMock.Setup(chan => chan.BasicPublish(
                exchangeName,
                routingKey,
                false,
                propsMock.Object,
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == messageBody)
             ));

            target.SendMessage(message);

            propsMock.VerifyAll();
            contextMock.VerifyAll();
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void SendMessage_ShouldUseDefaultValuesInBasicPropertiesWhenNotProvided()
        {
            var routingKey = "routingKey";
            var messageBody = "Test message";

            var message = new EventMessage(
                routingKey: routingKey,
                message: messageBody
            );

            var propsMock = new Mock<BasicProperties>(MockBehavior.Strict);
            propsMock.SetupSet(props => props.Type = "");
            propsMock.SetupSet(props => props.CorrelationId = It.IsAny<string>());
            propsMock.SetupSet(props => props.Timestamp = It.IsAny<AmqpTimestamp>());

            contextMock.Setup(ctx => ctx.ExchangeName).Returns(exchangeName);
            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsMock.Object);
            channelMock.Setup(chan => chan.BasicPublish(
                exchangeName,
                routingKey,
                false,
                propsMock.Object,
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == messageBody)
             ));

            target.SendMessage(message);

            propsMock.VerifyAll();
            contextMock.VerifyAll();
            channelMock.VerifyAll();
        }

        [TestMethod]
        public void SendMessage_ShouldThrowExceptionWhenDisposed()
        {
            var message = new EventMessage("RoutingKey", "message", "type");
            channelMock.Setup(chan => chan.Dispose());

            target.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => target.SendMessage(message));
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            channelMock.Setup(chan => chan.Dispose());

            target.Dispose();
            target.Dispose(); // Don't call dispose the second time

            channelMock.Verify(chan => chan.Dispose(), Times.Once);
        }
    }
}
