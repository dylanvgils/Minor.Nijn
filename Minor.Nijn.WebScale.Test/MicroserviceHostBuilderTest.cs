using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses;
using Moq;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using Minor.Nijn.WebScale.Test.InvalidTestClasses;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class MicroserviceHostBuilderTest
    {
        private Mock<IMessageReceiver> _receiverMock;
        private Mock<IBusContext<IConnection>> _busContextMock;

        private MicroserviceHostBuilder _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _receiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            _receiverMock.Setup(recv => recv.DeclareQueue());
            _receiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            _busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            _busContextMock.Setup(ctx => ctx.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(_receiverMock.Object);

            _target = new MicroserviceHostBuilder();
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHost()
        {
            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            var result = _target.WithContext(busContextMock.Object).CreateHost();
            Assert.IsInstanceOfType(result, typeof(MicroserviceHost));
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWithOneEventListenerWhenCalledWithAddEventListener()
        {
            var result = _target.AddListener<ProductEventListener>().WithContext(_busContextMock.Object).CreateHost();

            var listener = result.EventListeners.First();
            Assert.AreEqual(1, result.EventListeners.Count());
            Assert.AreEqual(TestClassesConstants.ProductEventListenerQueueName, listener.QueueName);
            Assert.AreEqual(TestClassesConstants.ProductEventHandlerTopic, listener.TopicExpressions.First());
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWithOneCommandListenerWhenCalledWithAddEventListener()
        {
            var result = _target.AddListener<OrderCommandListener>().WithContext(_busContextMock.Object).CreateHost();

            var listener = result.CommandListeners.First();
            Assert.AreEqual(1, result.CommandListeners.Count());
            Assert.AreEqual(TestClassesConstants.OrderCommandListenerQueueName, listener.QueueName);
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWithEventListenersWhenCalledWithUseConventions()
        {
            var result = _target.UseConventions().WithContext(_busContextMock.Object).CreateHost();

            var listeners = result.EventListeners;
            Assert.AreEqual(2, listeners.Count());
            Assert.IsTrue(listeners.Any(l => l.QueueName == TestClassesConstants.ProductEventListenerQueueName && l.TopicExpressions.Contains(TestClassesConstants.ProductEventHandlerTopic)));
            Assert.IsTrue(listeners.Any(l => l.QueueName == TestClassesConstants.OrderEventListenerQueueName && l.TopicExpressions.Contains(TestClassesConstants.OrderEventHandlerTopic)));
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWithCommandListenersWhenCalledWithUseConventions()
        {
            var result = _target.UseConventions().WithContext(_busContextMock.Object).CreateHost();

            var listeners = result.CommandListeners;
            Assert.AreEqual(2, listeners.Count());
            Assert.IsTrue(listeners.Any(l => l.QueueName == TestClassesConstants.ProductCommandListenerQueueName));
            Assert.IsTrue(listeners.Any(l => l.QueueName == TestClassesConstants.OrderCommandListenerQueueName));
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenEventMethodHasToManyParameters()
        {
            Action action = () => { _target.AddListener<InvalidEventParametersLength>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Method 'ToManyParameters' in type 'InvalidEventParametersLength' has to many parameters", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenCommandMethodHasToManyParameters()
        {
            Action action = () => { _target.AddListener<InvalidCommandParameterLength>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Method 'ToManyParameters' in type 'InvalidCommandParameterLength' has to many parameters", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenParameterTypeNotADerivedTypeOfDomainEvent()
        {
            Action action = () => { _target.AddListener<InvalidEventParameterType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid parameter type in 'ParameterTypeInvalid', parameter has to be derived type of DomainEvent", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenParameterTypeNotADerivedTypeOfDomainCommand()
        {
            Action action = () => { _target.AddListener<InvalidCommandParameterType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid parameter type in 'ParameterTypeInvalid', parameter has to be derived type of DomainCommand", ex.Message);
        }

        [TestMethod]
        public void SetLoggerFactory_ShouldSetTheLoggerFactoryForTheProject()
        {
            var factory = new LoggerFactory();
            _target.SetLoggerFactory(factory);
            Assert.AreEqual(NijnWebScaleLogger.LoggerFactory, factory);
        }
    }
}
