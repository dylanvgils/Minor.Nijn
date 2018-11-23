using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using Minor.Nijn.WebScale.Events;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private Mock<IBusContext<IConnection>> busContextMock;
        private Mock<IEventListener> eventListenerMock;

        private MicroserviceHost target;

        [TestInitialize]
        public void BeforeEach()
        {
            eventListenerMock = new Mock<IEventListener>(MockBehavior.Strict);
            var listeners = new List<IEventListener> { eventListenerMock.Object };

            busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            target = new MicroserviceHost(busContextMock.Object, listeners);
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldCreateMessageReceivers()
        {
            eventListenerMock.Setup(l => l.StartListening(busContextMock.Object));

            target.RegisterEventListeners();

            busContextMock.VerifyAll();
            eventListenerMock.VerifyAll();
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldThrowExceptionWhenCalledForTheSecondTime()
        {
            eventListenerMock.Setup(l => l.StartListening(busContextMock.Object));

            target.RegisterEventListeners();
            Action action = () =>
            {
                target.RegisterEventListeners();
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("Event listeners already registered", ex.Message);
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            eventListenerMock.Setup(e => e.Dispose());
            busContextMock.Setup(ctx => ctx.Connection.Dispose());

            target.Dispose();

            eventListenerMock.VerifyAll();
            busContextMock.VerifyAll();
        }
    }
}
