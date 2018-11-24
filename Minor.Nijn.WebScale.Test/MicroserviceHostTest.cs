using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Events;
using Minor.Nijn.WebScale.Test.TestClasses;

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
            var serviceCollection = new ServiceCollection();

            eventListenerMock = new Mock<IEventListener>(MockBehavior.Strict);
            var listeners = new List<IEventListener> { eventListenerMock.Object };

            busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            target = new MicroserviceHost(busContextMock.Object, listeners, serviceCollection);
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldCreateMessageReceivers()
        {
            eventListenerMock.Setup(l => l.StartListening(target));

            target.RegisterListeners();

            busContextMock.VerifyAll();
            eventListenerMock.VerifyAll();
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldThrowExceptionWhenCalledForTheSecondTime()
        {
            eventListenerMock.Setup(l => l.StartListening(target));

            target.RegisterListeners();
            Action action = () =>
            {
                target.RegisterListeners();
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("Event listeners already registered", ex.Message);
        }

        [TestMethod]
        public void CreateInstance_ShouldReturnInstanceOfType()
        {
            var type = typeof(OrderEventListener);
            var result = target.CreateInstance(type);
            Assert.IsInstanceOfType(result, typeof(OrderEventListener));
        }

        [TestMethod]
        public void MicroserviceHost_ShouldRegisterPublisherAndContextDependenciesByDefault()
        {
            var testBusContext = new TestBusContextBuilder().CreateTestContext();
            var target = new MicroserviceHost(testBusContext, new List<IEventListener>(), new ServiceCollection());

            var busContext = target.ServiceProvider.GetService<IBusContext<IConnection>>();
            var eventPublisher = target.ServiceProvider.GetService<IEventPublisher>();

            Assert.IsInstanceOfType(busContext, typeof(TestBusContext));
            Assert.IsInstanceOfType(eventPublisher, typeof(EventPublisher));
        }


        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            eventListenerMock.Setup(e => e.Dispose());
            busContextMock.Setup(ctx => ctx.Dispose());

            target.Dispose();

            eventListenerMock.VerifyAll();
            busContextMock.VerifyAll();
        }
    }
}
