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

        private RabbitMQCommandReceiver target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IRabbitMQBusContext>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);

            contextMock.Setup(ctx => ctx.Connection.CreateModel()).Returns(channelMock.Object);

            target = new RabbitMQCommandReceiver(contextMock.Object, queueName);
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
            channelMock.Setup(chan =>
                    chan.BasicConsume(queueName, false, "", false, false, null, It.IsAny<EventingBasicConsumer>()))
                .Returns("Ok");

            CommandMessage message = null;
            target.StartReceivingCommands(eventMessage => message = eventMessage);

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