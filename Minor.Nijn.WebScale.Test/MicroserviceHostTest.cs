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
        private Mock<IBusContext<IConnection>> _busContextMock;
        private Mock<IEventListener> _eventListenerMock;
        private Mock<ICommandListener> _commandListenerMock;

        private MicroserviceHost _target;

        [TestInitialize]
        public void BeforeEach()
        {
            var serviceCollection = new ServiceCollection();

            _eventListenerMock = new Mock<IEventListener>(MockBehavior.Strict);
            var eventListeners = new List<IEventListener> { _eventListenerMock.Object };

            _commandListenerMock = new Mock<ICommandListener>(MockBehavior.Strict);
            var commandListeners = new List<ICommandListener> { _commandListenerMock.Object };

            _busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            _target = new MicroserviceHost(_busContextMock.Object, eventListeners, commandListeners, serviceCollection);
        }

        [TestMethod]
        public void RegisterListeners_ShouldCreateMessageReceivers()
        {
            _eventListenerMock.Setup(l => l.RegisterListener(_target));

            _target.RegisterListeners();

            _busContextMock.VerifyAll();
            _eventListenerMock.VerifyAll();
            _commandListenerMock.VerifyAll();
        }

        [TestMethod]
        public void StartListening_ShouldCreateMessageReceivers()
        {
            _eventListenerMock.Setup(l => l.RegisterListener(_target));
            _eventListenerMock.Setup(l => l.StartListening());

            _commandListenerMock.Setup(c => c.StartListening(_target));

            _target.StartListening();

            _busContextMock.VerifyAll();
            _eventListenerMock.VerifyAll();
            _commandListenerMock.VerifyAll();
        }

        [TestMethod]
        public void RegisterListeners_ShouldThrowExceptionWhenCalledForTheSecondTime()
        {
            _eventListenerMock.Setup(l => l.RegisterListener(_target));

            _target.RegisterListeners();
            Action action = () =>
            {
                _target.RegisterListeners();
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("EventListeners already registered", ex.Message);
        }

        [TestMethod]
        public void StartListening_ShouldThrowExceptionWhenCalledForTheSecondTime()
        {
            _eventListenerMock.Setup(l => l.RegisterListener(_target));
            _eventListenerMock.Setup(l => l.StartListening());

            _commandListenerMock.Setup(c => c.StartListening(_target));

            _target.StartListening();
            Action action = () =>
            {
                _target.StartListening();
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("Listeners already listening", ex.Message);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void RegisterEventListeners_ShouldThrowExceptionWhenDisposed()
        {
            _eventListenerMock.Setup(e => e.Dispose());
            _commandListenerMock.Setup(c => c.Dispose());
            _busContextMock.Setup(ctx => ctx.Dispose());

            _target.Dispose();
            _target.RegisterListeners();
        }

        [TestMethod]
        public void CreateInstance_ShouldReturnInstanceOfType()
        {
            var type = typeof(OrderEventListener);
            var result = _target.CreateInstance(type);
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

            Assert.AreEqual(testBusContext, busContext);
            Assert.IsInstanceOfType(eventPublisher, typeof(EventPublisher));
            Assert.IsInstanceOfType(commandPublisher, typeof(CommandPublisher));
        }

        [TestMethod]
        public void MicroserviceHost_ShouldNotRegisterPublisherAndContextDependenciesWhenServiceCollectionIsProvided()
        {
            var testBusContext = new TestBusContextBuilder().CreateTestContext();

            var services = new ServiceCollection();
            services.AddSingleton<IBusContext<IConnection>>(testBusContext);
            services.AddTransient<ICommandPublisher, CommandPublisher>();
            services.AddTransient<IEventPublisher, EventPublisher>();

            var target = new MicroserviceHost(testBusContext, new List<IEventListener>(), new List<ICommandListener>(), services);

            var busContext = target.ServiceProvider.GetService<IBusContext<IConnection>>();
            var eventPublisher = target.ServiceProvider.GetService<IEventPublisher>();
            var commandPublisher = target.ServiceProvider.GetService<ICommandPublisher>();

            Assert.AreEqual(testBusContext, busContext);
            Assert.IsInstanceOfType(eventPublisher, typeof(EventPublisher));
            Assert.IsInstanceOfType(commandPublisher, typeof(CommandPublisher));
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            _eventListenerMock.Setup(e => e.Dispose());
            _commandListenerMock.Setup(c => c.Dispose());
            _busContextMock.Setup(ctx => ctx.Dispose());

            _target.Dispose();
            _target.Dispose(); // Don't call dispose the second time

            _eventListenerMock.VerifyAll();
            _commandListenerMock.VerifyAll();
            _busContextMock.Verify(ctx => ctx.Dispose(), Times.Once);
        }
    }
}
