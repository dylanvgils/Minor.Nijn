using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQCommandSenderTest
    {
        private Mock<EventingBasicConsumerFactory> eventingBasicConsumerFactoryMock;
        private Mock<IRabbitMQBusContext> contextMock;
        private Mock<IModel> channelMock;

        private RabbitMQCommandSender target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IRabbitMQBusContext>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);
            eventingBasicConsumerFactoryMock = new Mock<EventingBasicConsumerFactory>(MockBehavior.Strict);

            contextMock.Setup(ctx => ctx.Connection.CreateModel()).Returns(channelMock.Object);

            target = new RabbitMQCommandSender(contextMock.Object, eventingBasicConsumerFactoryMock.Object);
        }

        [TestMethod]
        public void CommandReceiver_ShouldBeCreatedWithCorrectParameters()
        {
            contextMock.VerifyAll();

            Assert.IsNotNull(target);
            Assert.IsNotNull(target.Channel);
        }
        
        [TestMethod]
        public void SendCommandAsync_ShouldSentCommandAndReturnTaskWithResult()
        {
            var consumer = new EventingBasicConsumer(channelMock.Object);

            ulong deliveryTag = 1;
            var type = "type";
            var correlationId = "correlationId";
            var replyQueueName = "queueName";
            var routingKey = "routingKey";
            var timestamp = DateTime.Now.Ticks;
            var requestCommandBody = "Request message";
            var replyCommandMessage = "Reply message";
            var requestCommand = new RequestCommandMessage(requestCommandBody, type, correlationId, routingKey, timestamp);

            var basicPropsMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            basicPropsMock.SetupSet(props => props.ReplyTo = replyQueueName);
            basicPropsMock.SetupSet(props => props.CorrelationId = correlationId);
            basicPropsMock.SetupSet(props => props.Type = type);
            basicPropsMock.SetupSet(props => props.Timestamp = It.IsAny<AmqpTimestamp>());
            
            var replyPropsMock = new Mock<IBasicProperties>();
            replyPropsMock.SetupGet(props => props.CorrelationId).Returns(correlationId);
            replyPropsMock.SetupGet(props => props.Type).Returns(type);
            replyPropsMock.SetupGet(props => props.Timestamp).Returns(new AmqpTimestamp(timestamp));
            
            channelMock.Setup(chan => chan.QueueDeclare("", false, true, true, null))
                .Returns(new QueueDeclareOk(replyQueueName, 0, 0));

            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(basicPropsMock.Object);
            
            channelMock.Setup(chan => chan.BasicConsume(replyQueueName, true, "", false, false, null, consumer))
                .Returns("Ok");
            
            channelMock.Setup(chan => chan.BasicPublish(
                "", 
                routingKey, 
                false, 
                basicPropsMock.Object, 
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == requestCommandBody)
            ));

            eventingBasicConsumerFactoryMock.Setup(fact => fact.CreateEventingBasicConsumer(channelMock.Object))
                .Returns(consumer);

            var result = target.SendCommandAsync(requestCommand);
            Thread.Sleep(50);
            consumer.HandleBasicDeliver("", deliveryTag, false, "",  routingKey,  replyPropsMock.Object, Encoding.UTF8.GetBytes(replyCommandMessage));
            var replyCommand = result.Result;
            
            basicPropsMock.VerifyAll();
            replyPropsMock.VerifyAll();
            channelMock.VerifyAll();
            contextMock.VerifyAll();
            
            Assert.AreEqual(replyCommand.Message, replyCommandMessage);
            Assert.AreEqual(replyCommand.Type, type);
            Assert.AreEqual(replyCommand.CorrelationId, correlationId);
            Assert.AreEqual(replyCommand.Timestamp, timestamp);
        }

        [TestMethod]
        public void SendCommandAsync_ShouldUseDefaultValuesInBasicPropertiesWhenNotProvided()
        {
            var consumer = new EventingBasicConsumer(channelMock.Object);

            ulong deliveryTag = 1;
            var type = "type";
            var correlationId = "correlationId";
            var replyQueueName = "queueName";
            var routingKey = "routingKey";
            var requestCommandBody = "Request message";
            var replyCommandMessage = "Reply message";
            var requestCommand = new RequestCommandMessage(requestCommandBody, type, correlationId, routingKey);

            var basicPropsMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            basicPropsMock.SetupSet(props => props.ReplyTo = replyQueueName);
            basicPropsMock.SetupSet(props => props.CorrelationId = correlationId);
            basicPropsMock.SetupSet(props => props.Type = type);
            basicPropsMock.SetupSet(props => props.Timestamp = It.IsAny<AmqpTimestamp>());

            var replyPropsMock = new Mock<IBasicProperties>();
            replyPropsMock.SetupGet(props => props.CorrelationId).Returns(correlationId);
            replyPropsMock.SetupGet(props => props.Type).Returns(type);
            replyPropsMock.SetupGet(props => props.Timestamp).Returns(new AmqpTimestamp());

            channelMock.Setup(chan => chan.QueueDeclare("", false, true, true, null))
                .Returns(new QueueDeclareOk(replyQueueName, 0, 0));

            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(basicPropsMock.Object);

            channelMock.Setup(chan => chan.BasicConsume(replyQueueName, true, "", false, false, null, consumer))
                .Returns("Ok");

            channelMock.Setup(chan => chan.BasicPublish(
                "",
                routingKey,
                false,
                basicPropsMock.Object,
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == requestCommandBody)
            ));

            eventingBasicConsumerFactoryMock.Setup(fact => fact.CreateEventingBasicConsumer(channelMock.Object))
                .Returns(consumer);

            var result = target.SendCommandAsync(requestCommand);
            Thread.Sleep(50);
            consumer.HandleBasicDeliver("", deliveryTag, false, "", routingKey, replyPropsMock.Object, Encoding.UTF8.GetBytes(replyCommandMessage));
            var replyCommand = result.Result;

            basicPropsMock.VerifyAll();
            replyPropsMock.VerifyAll();
            channelMock.VerifyAll();
            contextMock.VerifyAll();

            Assert.AreEqual(replyCommand.Message, replyCommandMessage);
            Assert.AreEqual(replyCommand.Type, type);
            Assert.AreEqual(replyCommand.CorrelationId, correlationId);
        }

        [TestMethod]
        public void SendCommandAsync_ShouldThrowCommandTimeOutExceptionAfter_5Seconds()
        {
            var consumer = new EventingBasicConsumer(channelMock.Object);

            ulong deliveryTag = 1;
            var type = "type";
            var correlationId = "correlationId";
            var replyQueueName = "queueName";
            var routingKey = "routingKey";
            var requestCommandBody = "Request message";
            var replyCommandMessage = "Reply message";
            var requestCommand = new RequestCommandMessage(requestCommandBody, type, correlationId, routingKey);

            var basicPropsMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            basicPropsMock.SetupSet(props => props.ReplyTo = replyQueueName);
            basicPropsMock.SetupSet(props => props.CorrelationId = correlationId);
            basicPropsMock.SetupSet(props => props.Type = type);
            basicPropsMock.SetupSet(props => props.Timestamp = It.IsAny<AmqpTimestamp>());
            
            var replyPropsMock = new Mock<IBasicProperties>();
            replyPropsMock.SetupGet(props => props.CorrelationId).Returns("wrongId");

            
            channelMock.Setup(chan => chan.QueueDeclare("", false, true, true, null))
                .Returns(new QueueDeclareOk(replyQueueName, 0, 0));

            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(basicPropsMock.Object);
            
            channelMock.Setup(chan => chan.BasicConsume(replyQueueName, true, "", false, false, null, consumer))
                .Returns("Ok");
            
            channelMock.Setup(chan => chan.BasicPublish(
                "", 
                routingKey, 
                false, 
                basicPropsMock.Object, 
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == requestCommandBody)
            ));

            eventingBasicConsumerFactoryMock.Setup(fact => fact.CreateEventingBasicConsumer(channelMock.Object))
                .Returns(consumer);

            var result = target.SendCommandAsync(requestCommand);
            Thread.Sleep(50);
            consumer.HandleBasicDeliver("", deliveryTag, false, "",  routingKey,  replyPropsMock.Object, Encoding.UTF8.GetBytes(replyCommandMessage));
                
            basicPropsMock.VerifyAll();
            replyPropsMock.VerifyAll();
            channelMock.VerifyAll();
            contextMock.VerifyAll();

            while (!result.IsCompleted) { }
            Assert.IsTrue(result.IsFaulted);
        }

        [TestMethod]
        public void SendMessageAsync_ShouldThrowExceptionWhenDisposed()
        {
            var command = new RequestCommandMessage("RoutingKey", "type", "id");
            channelMock.Setup(chan => chan.Dispose());

            target.Dispose();

            Assert.ThrowsException<ObjectDisposedException>(() => target.SendCommandAsync(command));
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