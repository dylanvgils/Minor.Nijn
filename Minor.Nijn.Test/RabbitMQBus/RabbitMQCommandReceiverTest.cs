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
            channelMock.Setup(chan => chan.QueueDeclare(queueName, false, false, false, null))
                .Returns(new QueueDeclareOk(queueName, 0, 0));

            channelMock.Setup(chan => chan.BasicQos(0, 1, false));

            target.DeclareCommandQueue();

            channelMock.Verify();
        }

        [TestMethod]
        public void StartReceivingCommands_ShouldStartListeningForCommands()
        {
            var consumer = new EventingBasicConsumer(channelMock.Object);

            var type = "Type";
            var correlationId = "CorrelationId";
            var messageId = "messageId";
            var replyToQueueName = "ReplyToQueueName";
            var requestMessageBody = "Test message";
            var replyMessageBody = "Reply message";
            var replyMessage = new CommandMessage(replyMessageBody, type, correlationId);
            
            var propsRequestMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsRequestMock.SetupGet(props => props.Type).Returns(type);
            propsRequestMock.SetupGet(props => props.ReplyTo).Returns(replyToQueueName);
            propsRequestMock.SetupGet(props => props.CorrelationId).Returns(correlationId);
            propsRequestMock.SetupGet(props => props.MessageId).Returns(messageId);

            var propsReplyMock = new Mock<IBasicProperties>(MockBehavior.Strict);
            propsReplyMock.SetupSet(props => props.CorrelationId = correlationId);

            channelMock.Setup(chan => chan.CreateBasicProperties()).Returns(propsReplyMock.Object);
            
            channelMock.Setup(chan =>
                    chan.BasicConsume(queueName, false, "", false, false, null, consumer))
                .Returns("Ok");

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
            
            Assert.AreEqual(requestMessage.Message, requestMessageBody);
            Assert.AreEqual(replyMessage.Type, type);
            Assert.AreEqual(requestMessage.CorrelationId, correlationId);
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