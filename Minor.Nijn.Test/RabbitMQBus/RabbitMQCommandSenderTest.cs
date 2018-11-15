using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQCommandSenderTest
    {
        private Mock<IRabbitMQBusContext> contextMock;
        private Mock<IModel> channelMock;

        private RabbitMQCommandSender target;

        [TestInitialize]
        public void BeforeEach()
        {
            contextMock = new Mock<IRabbitMQBusContext>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);

            contextMock.Setup(ctx => ctx.Connection.CreateModel()).Returns(channelMock.Object);

            target = new RabbitMQCommandSender(contextMock.Object);
        }

        [TestMethod]
        public void CommandReceiver_ShouldBeCreatedWithCorrectParameters()
        {
            contextMock.VerifyAll();

            Assert.IsNotNull(target);
            Assert.IsNotNull(target.Channel);
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