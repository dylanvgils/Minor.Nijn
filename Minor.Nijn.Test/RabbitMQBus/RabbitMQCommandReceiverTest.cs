using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQCommandReceiverTest
    {
        private string queueName = "commandQueue";

        private Mock<IRabbitMQBusContext> contextMock;
        private Mock<IModel> channelMock;
        private Mock<EventingBasicConsumerFactory> eventingBasicConsumerFactoryMock;
        
        private RabbitMQCommandReceiver target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IRabbitMQBusContext>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);
            eventingBasicConsumerFactoryMock = new Mock<EventingBasicConsumerFactory>(MockBehavior.Strict);

            contextMock.Setup(ctx => ctx.Connection.CreateModel()).Returns(channelMock.Object);

            target = new RabbitMQCommandReceiver(contextMock.Object, queueName, eventingBasicConsumerFactoryMock.Object);
        }

        [TestMethod]
        public void CommandReceiver_ShouldBeCreatedWithCorrectParameters()
        {
            contextMock.VerifyAll();

            Assert.IsNotNull(target);
            Assert.AreEqual(queueName, target.QueueName);
            Assert.IsNotNull(target.Channel);
        }

        [TestMethod]
        public void DeclareCommandQueue_ShouldDeclareNewCommandQueue()
        {
            channelMock.Setup(chan => chan.QueueDeclare(queueName, false, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            channelMock.Setup(chan => chan.BasicQos(0, 1, false));

            target.DeclareCommandQueue();

            channelMock.Verify();
        }

        [TestMethod]
        public void DeclareCommandQueue_ShouldThrowExceptionWhenAlreadyDeclared()
        {
            channelMock.Setup(chan => chan.QueueDeclare(queueName, false, false, true, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            channelMock.Setup(chan => chan.BasicQos(0, 1, false));

            target.DeclareCommandQueue();
            Action action = () => { target.DeclareCommandQueue(); };

            channelMock.Verify();

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual($"Queue with name: {queueName} is already declared", ex.Message);
        }

        [TestMethod]
        public void DeclareCommandQueue_ShouldThrowExceptionWhenDisposed()
        {
            channelMock.Setup(chan => chan.Dispose());
            target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => target.DeclareCommandQueue());
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldStartListeningForCommands()
        {
            var consumer = new EventingBasicConsumer(channelMock.Object);

            var type = "Type";
            var correlationId = "CorrelationId";
            var timestamp = DateTime.Now.Ticks;
            var replyToQueueName = "ReplyToQueueName";
            var requestMessageBody = "Test message";
            var replyMessageBody = "Reply message";
            var replyMessage = new ResponseCommandMessage(replyMessageBody, type, correlationId, timestamp);
            
            var propsRequestMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsRequestMock.SetupGet(props => props.Type).Returns(type);
            propsRequestMock.SetupGet(props => props.ReplyTo).Returns(replyToQueueName);
            propsRequestMock.SetupGet(props => props.CorrelationId).Returns(correlationId);
            propsRequestMock.SetupGet(props => props.Timestamp).Returns(new AmqpTimestamp(timestamp));

            var propsReplyMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsReplyMock.SetupSet(props => props.CorrelationId = correlationId);
            propsReplyMock.SetupSet(props => props.Type = type);
            propsReplyMock.SetupSet(props => props.Timestamp = new AmqpTimestamp(timestamp));

            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsReplyMock.Object);
            channelMock.Setup(chan =>
                    chan.BasicConsume(queueName, false, "", false, false, null, consumer))
                .Returns("Ok");
            channelMock.Setup(chan => chan.QueueDeclare(queueName, false, false, true, null)).Returns(new QueueDeclareOk(queueName, 0, 0));
            channelMock.Setup(chan => chan.BasicQos(0, 1, false));

            channelMock.Setup(chan => chan.BasicPublish(
                "", 
                replyToQueueName, 
                false, 
                propsReplyMock.Object, 
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == replyMessageBody)
            ));

            channelMock.Setup(chan => chan.BasicAck(1, false));
            
            eventingBasicConsumerFactoryMock.Setup(fact => fact.CreateEventingBasicConsumer(channelMock.Object))
                .Returns(consumer);

            CommandMessage requestMessage = null;
            target.DeclareCommandQueue();
            target.StartReceivingCommands(eventMessage =>
            {
                requestMessage = eventMessage;
                return replyMessage;
            });
            
            consumer.HandleBasicDeliver("", 1, false, "",  "routingKey",  propsRequestMock.Object, Encoding.UTF8.GetBytes(requestMessageBody));

            propsRequestMock.VerifyAll();
            propsReplyMock.VerifyAll();
            channelMock.VerifyAll();
            eventingBasicConsumerFactoryMock.VerifyAll();
            contextMock.VerifyAll();
            
            Assert.AreEqual(requestMessageBody, requestMessage.Message);
            Assert.AreEqual(type, replyMessage.Type);
            Assert.AreEqual(correlationId, requestMessage.CorrelationId);
            Assert.AreEqual(timestamp, replyMessage.Timestamp);
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldUseDefaultValuesInBasicPropertiesWhenNotProvided()
        {
            var consumer = new EventingBasicConsumer(channelMock.Object);

            var type = "Type";
            var correlationId = "CorrelationId";
            var replyToQueueName = "ReplyToQueueName";
            var requestMessageBody = "Test message";
            var replyMessageBody = "Reply message";
            var replyMessage = new ResponseCommandMessage(replyMessageBody, type, correlationId);

            var propsRequestMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsRequestMock.SetupGet(props => props.Type).Returns(type);
            propsRequestMock.SetupGet(props => props.ReplyTo).Returns(replyToQueueName);
            propsRequestMock.SetupGet(props => props.CorrelationId).Returns(correlationId);
            propsRequestMock.SetupGet(props => props.Timestamp).Returns(new AmqpTimestamp());

            var propsReplyMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsReplyMock.SetupSet(props => props.CorrelationId = correlationId);
            propsReplyMock.SetupSet(props => props.Type = type);
            propsReplyMock.SetupSet(props => props.Timestamp = It.IsAny<AmqpTimestamp>());

            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsReplyMock.Object);
            channelMock.Setup(chan =>
                    chan.BasicConsume(queueName, false, "", false, false, null, consumer))
                .Returns("Ok");
            channelMock.Setup(chan => chan.QueueDeclare(queueName, false, false, true, null)).Returns(new QueueDeclareOk(queueName, 0, 0));
            channelMock.Setup(chan => chan.BasicQos(0, 1, false));

            channelMock.Setup(chan => chan.BasicPublish(
                "",
                replyToQueueName,
                false,
                propsReplyMock.Object,
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == replyMessageBody)
            ));

            channelMock.Setup(chan => chan.BasicAck(1, false));

            eventingBasicConsumerFactoryMock.Setup(fact => fact.CreateEventingBasicConsumer(channelMock.Object))
                .Returns(consumer);

            CommandMessage requestMessage = null;
            target.DeclareCommandQueue();
            target.StartReceivingCommands(eventMessage =>
            {
                requestMessage = eventMessage;
                return replyMessage;
            });

            consumer.HandleBasicDeliver("", 1, false, "", "routingKey", propsRequestMock.Object, Encoding.UTF8.GetBytes(requestMessageBody));

            propsRequestMock.VerifyAll();
            propsReplyMock.VerifyAll();
            channelMock.VerifyAll();
            eventingBasicConsumerFactoryMock.VerifyAll();
            contextMock.VerifyAll();

            Assert.AreEqual(requestMessageBody, requestMessage.Message);
            Assert.AreEqual(type, replyMessage.Type);
            Assert.AreEqual(correlationId, requestMessage.CorrelationId);
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldThrowBusConfigurationExceptionWhenQueueIsNotDeclared()
        {
            var replyMessage = new ResponseCommandMessage("reply message", "type", "correlationId");

            CommandMessage requestMessage = null;
            Action action = () =>
            {
                target.StartReceivingCommands(eventMessage =>
                {
                    requestMessage = eventMessage;
                    return replyMessage;
                });
            };

            Assert.IsNull(requestMessage);
            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual($"Queue with name: {queueName} is not declared", ex.Message);
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldThrowExceptionWhenDisposed()
        {
            channelMock.Setup(chan => chan.Dispose());
            target.Dispose();
            Assert.ThrowsException<ObjectDisposedException>(() => target.StartReceivingCommands((m) => new ResponseCommandMessage("message", "type", "id")));
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