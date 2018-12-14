using ConsoleAppExample.Domain;
using ConsoleAppExample.Listeners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Events;
using Moq;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleAppExample.Test.Listeners
{
    [TestClass]
    public class CommandListenerTest
    {
        private Mock<IEventPublisher> _eventPublisherMock;
        private CommandListener _target;

        [TestInitialize]
        public void BeforeEach()
        {
           _eventPublisherMock = new Mock<IEventPublisher>(MockBehavior.Strict);

            _target = new CommandListener(_eventPublisherMock.Object);
        }

        [TestMethod]
        public void HandleSayHelloCommand_ShouldPublishEvent()
        {
            var requestCommand = new SayHelloCommand("Testje", "RoutingKey");

            SaidHelloEvent helloEvent = null;
            _eventPublisherMock.Setup(e => e.Publish(It.IsAny<SaidHelloEvent>()))
                .Callback((DomainEvent e) => helloEvent = (SaidHelloEvent) e);

            var result = _target.HandleSayHelloCommand(requestCommand);


            _eventPublisherMock.VerifyAll();

            Assert.AreEqual("Hello, Testje", result);

            Assert.IsNotNull(helloEvent);
            Assert.AreEqual("SaidHello", helloEvent.RoutingKey);
            Assert.AreEqual("Hello, Testje", helloEvent.Message);
        }

        [TestMethod]
        public async Task HandleWithAsyncCommandResponse_ShouldHandleCommandRequest()
        {
            var requestCommand = new SayHelloCommand("Testje", "RoutingKey");
            var stopwatch = new Stopwatch();

            stopwatch.Start();
            var result = await _target.HandleWithAsyncCommandResponse(requestCommand);
            stopwatch.Stop();

            Assert.IsTrue(stopwatch.ElapsedMilliseconds >= 1000, "Elapsed time should be at least 1 second");
            Assert.AreEqual("Hello, Testje", result);
        }
    }
}