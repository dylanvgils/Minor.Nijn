using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Minor.Nijn.WebScale.Events;
using Minor.Nijn.WebScale.Test.TestClasses;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using Minor.Nijn.WebScale.Commands;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private Mock<IBusContext<IConnection>> busContextMock;
        private Mock<IEventListener> eventListenerMock;
        private Mock<ICommandListener> commandListenerMock;

        private MicroserviceHost target;

        [TestInitialize]
        public void BeforeEach()
        {
            var serviceCollection = new ServiceCollection();

            eventListenerMock = new Mock<IEventListener>(MockBehavior.Strict);
            var eventListeners = new List<IEventListener> { eventListenerMock.Object };

            commandListenerMock = new Mock<ICommandListener>(MockBehavior.Strict);
            var commandListeners = new List<ICommandListener> { commandListenerMock.Object };

            busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            target = new MicroserviceHost(busContextMock.Object, eventListeners, commandListeners, serviceCollection);
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldCreateMessageReceivers()
        {
            eventListenerMock.Setup(l => l.StartListening(target));
            commandListenerMock.Setup(c => c.StartListening(target));

            target.RegisterListeners();

            busContextMock.VerifyAll();
            eventListenerMock.VerifyAll();
            commandListenerMock.VerifyAll();
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldThrowExceptionWhenCalledForTheSecondTime()
        {
            eventListenerMock.Setup(l => l.StartListening(target));
            commandListenerMock.Setup(c => c.StartListening(target));

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
            var target = new MicroserviceHost(testBusContext, new List<IEventListener>(), new List<ICommandListener>(),  new ServiceCollection());

            var busContext = target.ServiceProvider.GetService<IBusContext<IConnection>>();
            var eventPublisher = target.ServiceProvider.GetService<IEventPublisher>();
            var commandPublisher = target.ServiceProvider.GetService<ICommandPublisher>();

            Assert.IsInstanceOfType(busContext, typeof(TestBusContext));
            Assert.IsInstanceOfType(eventPublisher, typeof(EventPublisher));
            Assert.IsInstanceOfType(commandPublisher, typeof(CommandPublisher));
        }


        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            eventListenerMock.Setup(e => e.Dispose());
            commandListenerMock.Setup(c => c.Dispose());
            busContextMock.Setup(ctx => ctx.Dispose());

            target.Dispose();
            target.Dispose(); // Don't call dispose the second time

            eventListenerMock.VerifyAll();
            commandListenerMock.VerifyAll();
            busContextMock.Verify(ctx => ctx.Dispose(), Times.Once);
        }
    }
}
