using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQMessageReceiver_Test
    {
        [TestMethod]
        public void ConsumerReceivedEventGetsFiredWhenMessageIsReceived()
        {
            var propsMock = new Mock<IBasicProperties>();
            var channelMock = new Mock<IModel>(MockBehavior.Strict);

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);

            connectionMock.Setup(r => r.CreateModel())
                       .Returns(channelMock.Object)
                       .Verifiable();

            var context = new RabbitMQBusContext(connectionMock.Object, "Testxchange1");
            var target = new RabbitMQMessageSender(context);


        }
    }
}
