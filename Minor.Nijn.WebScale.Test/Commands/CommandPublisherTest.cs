using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
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

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public async Task Publish_ShouldThrowExceptionWhenDisposed()
        {
            var commandSenderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            commandSenderMock.Setup(s => s.Dispose());

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(commandSenderMock.Object);

            var command = new AddOrderCommand("RoutinKey", new Order());
            var target = new CommandPublisher(contextMock.Object);

            target.Dispose();

            await target.Publish<long>(command);
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
            target.Dispose(); // Don't call dispose the second time

            senderMock.Verify(s => s.Dispose(), Times.Once);
        }
    }
}