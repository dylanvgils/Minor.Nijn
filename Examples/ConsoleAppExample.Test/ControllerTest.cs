using System.Threading.Tasks;
using ConsoleAppExample.Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Events;
using Moq;

namespace ConsoleAppExample.Test
{
    [TestClass]
    public class ControllerTest
    {
        private Mock<ICommandPublisher> _commandPublisherMock;
        private Mock<IEventPublisher> _eventPublisherMock;

        private Controller _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _commandPublisherMock = new Mock<ICommandPublisher>(MockBehavior.Strict);
            _eventPublisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);

            _target = new Controller(_commandPublisherMock.Object, _eventPublisherMock.Object);
        }

        [TestMethod]
        public async Task SayHello_ShouldPublishCommand()
        {
            var name = "Testje";

            SayHelloCommand result = null;
            _commandPublisherMock.Setup(p => p.Publish<string>(It.IsAny<SayHelloCommand>()))
                .Callback((DomainCommand c) => result = (SayHelloCommand) c)
                .ReturnsAsync("Response string");

            await _target.SayHello(name);

            _commandPublisherMock.VerifyAll();

            Assert.IsNotNull(result);
            Assert.AreEqual("ConsoleAppExampleCommandQueue", result.RoutingKey);
            Assert.AreEqual(name, result.Name);
        }

        [TestMethod]
        public async Task WhoopsExceptionThrown_ShouldHandleThrownException()
        {
            var exception = new CustomException("Some custom exception message");
            _commandPublisherMock.Setup(p => p.Publish<string>(It.IsAny<SayHelloCommand>()))
                .ThrowsAsync(exception);

            await _target.WhoopsExceptionThrown();

            _commandPublisherMock.VerifyAll();
        }
    }
}
