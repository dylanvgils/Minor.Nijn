using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Commands.Test
{
    [TestClass]
    public class CommandPublisherTest
    {
        [TestMethod]
        public void Publish_ShouldSendCommandAndReturnResult()
        {
            var input = 21;
            var command = new AddProductCommand("RoutingKey", input);

            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendCommandAsync(It.IsAny<RequestCommandMessage>()))
                .ReturnsAsync(new ResponseCommandMessage(JsonConvert.SerializeObject(input * 2), "int", "correlationId"));

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);

            var target = new CommandPublisher(contextMock.Object);
            var result = target.Publish<int>(command);

            Assert.AreEqual(42, result.Result);
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.Dispose());

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);
            
            var target = new CommandPublisher(contextMock.Object);
            target.Dispose();

            contextMock.VerifyAll();
            senderMock.VerifyAll();
        }
    }
}