using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Commands;
using Minor.Nijn.WebScale.Test.InvalidTestClasses;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Minor.Nijn.WebScale.TestClasses.Exceptions.Test;
using Moq;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using OrderCommandListener = Minor.Nijn.WebScale.Test.TestClasses.OrderCommandListener;
using OrderEventListener = Minor.Nijn.WebScale.Test.TestClasses.OrderEventListener;

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

        [TestCleanup]
        public void AfterEach()
        {
            CommandPublisher.ExceptionTypes = null;
        }

        [TestMethod]
        public void UseConventions_ShouldCreateEventListeners()
        {
            _target.UseConventions();

            var result = _target.EventListeners;
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(l => l.QueueName == TestClassesConstants.ProductEventListenerQueueName && l.TopicExpressions.Contains(TestClassesConstants.ProductEventHandlerTopic)));
            Assert.IsTrue(result.Any(l => l.QueueName == TestClassesConstants.OrderEventListenerQueueName && l.TopicExpressions.Contains(TestClassesConstants.OrderEventHandlerTopic)));
        }

        [TestMethod]
        public void UseConventions_ShouldCreateCommandListeners()
        {
            _target.UseConventions();

            var result = _target.CommandListeners;
            Assert.AreEqual(2, result.Count());
            Assert.IsTrue(result.Any(l => l.QueueName == TestClassesConstants.ProductCommandListenerQueueName));
            Assert.IsTrue(result.Any(l => l.QueueName == TestClassesConstants.OrderCommandListenerQueueName));
        }

        [TestMethod]
        public void ScanForExceptions_ShouldBuildExceptionTypeDictionaryForCallingAssembly()
        {
            _target.ScanForExceptions();
            Assert.IsTrue(_target.ExceptionTypes.Any(kv => kv.Key == typeof(TestException).Name), "Should contain TestException");
            Assert.IsTrue(_target.ExceptionTypes.Any(kv => kv.Key == typeof(BusConfigurationException).Name), "Should contain BusConfigurationException");
        }

        [TestMethod]
        public void ScanForExceptions_ShouldExcludeExceptionsFromScanWhenMatchesExclusionPattern()
        {
            _target.ScanForExceptions(new List<string> {"Minor.Nijn.WebScale"});
            Assert.IsFalse(_target.ExceptionTypes.Any(kv => kv.Key == typeof(TestException).Name), "Should not contain TestException");
            Assert.IsTrue(_target.ExceptionTypes.Any(kv => kv.Key == typeof(BusConfigurationException).Name), "Should contain BusConfigurationException");
        }

        [TestMethod]
        public void AddException_ShouldAddExceptionTypeToTheExceptionTypeDictionary()
        {
            _target.AddException<TestException>().WithContext(_busContextMock.Object).CreateHost();

            Assert.AreEqual(1, CommandPublisher.ExceptionTypes.Count);
            Assert.IsTrue(CommandPublisher.ExceptionTypes.ContainsKey(typeof(TestException).Name));
        }

        [TestMethod]
        public void AddException_ShouldThrowExceptionWhenExceptionAlreadyExists()
        {
            Action action = () =>
            {
                _target
                    .AddException<TestException>()
                    .AddException<TestException>()
                    .WithContext(_busContextMock.Object)
                    .CreateHost();
            };

            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual($"Unable to add exception to exception type dictionary, exception with name: {typeof(TestException).Name} already exists", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenEventMethodHasToManyParameters()
        {
            Action action = () => { _target.AddListener<InvalidEventParametersLength>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Method: 'ToManyParameters' in type: 'InvalidEventParametersLength' has to many parameters", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenCommandMethodHasToManyParameters()
        {
            Action action = () => { _target.AddListener<InvalidCommandParameterLength>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Method: 'ToManyParameters' in type: 'InvalidCommandParameterLength' has to many parameters", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenParameterTypeNotADerivedTypeOfDomainEvent()
        {
            Action action = () => { _target.AddListener<InvalidEventParameterType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid parameter type in method: 'ParameterTypeInvalid', parameter has to be derived type of DomainEvent", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenParameterTypeNotADerivedTypeOfDomainCommand()
        {
            Action action = () => { _target.AddListener<InvalidCommandParameterType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid parameter type in method: 'ParameterTypeInvalid', parameter has to be derived type of DomainCommand", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenReturnTypeIsInvalid()
        {
            Action action = () => { _target.AddListener<InvalidCommandListenerReturnType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid return of method: 'InvalidReturnType', returning a value from a CommandListener method is required", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenAsyncEventListenerMethodHasInvalidReturnType()
        {
            Action action = () => { _target.AddListener<InvalidEventListenerAsyncReturnType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid return type of method: InvalidAsyncReturnType, return type of async method should be Task", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldPassCorrectEventListenerInfoToEventListener()
        {
            _target.AddListener<OrderEventListener>();

            var result = _target.EventListeners.First().Meta;
            Assert.AreEqual(TestClassesConstants.OrderEventListenerQueueName, result.QueueName);
            Assert.AreEqual(TestClassesConstants.OrderEventHandlerTopic, result.TopicExpressions.First());
            Assert.AreEqual(typeof(OrderEventListener), result.Type);
            Assert.AreEqual(TestClassesConstants.OrderEventHandlerMethodName, result.Method.Name);
            Assert.AreEqual(typeof(OrderCreatedEvent), result.EventType);
            Assert.AreEqual(false, result.IsAsyncMethod);
        }

        [TestMethod]
        public void AddListener_ShouldPassCorrectAsyncEventListenerInfoToEventListener()
        {
            _target.AddListener<ProductEventListener>();

            var result = _target.EventListeners.First().Meta;
            Assert.AreEqual(TestClassesConstants.ProductEventListenerQueueName, result.QueueName);
            Assert.AreEqual(TestClassesConstants.ProductEventHandlerTopic, result.TopicExpressions.First());
            Assert.AreEqual(typeof(ProductEventListener), result.Type);
            Assert.AreEqual(TestClassesConstants.ProductEventHandlerMethodName, result.Method.Name);
            Assert.AreEqual(typeof(ProductAddedEvent), result.EventType);
            Assert.AreEqual(true, result.IsAsyncMethod);
        }

        [TestMethod]
        public void AddListener_ShouldPassCorrectCommandListenerInfoToCommandListener()
        {
            _target.AddListener<OrderCommandListener>();

            var result = _target.CommandListeners.First().Meta;
            Assert.AreEqual(TestClassesConstants.OrderCommandListenerQueueName, result.QueueName);
            Assert.AreEqual(typeof(OrderCommandListener), result.Type);
            Assert.AreEqual(TestClassesConstants.OrderCommandHandlerMethodName, result.Method.Name);
            Assert.AreEqual(typeof(AddOrderCommand), result.CommandType);
            Assert.AreEqual(false, result.IsAsyncMethod);
        }

        [TestMethod]
        public void AddListener_ShouldPassCorrectAsyncCommandListenerInfoToCommandListener()
        {
            _target.AddListener<ProductCommandListener>();

            var result = _target.CommandListeners.First().Meta;
            Assert.AreEqual(TestClassesConstants.ProductCommandListenerQueueName, result.QueueName);
            Assert.AreEqual(typeof(ProductCommandListener), result.Type);
            Assert.AreEqual(TestClassesConstants.ProductCommandHandlerMethodName, result.Method.Name);
            Assert.AreEqual(typeof(AddProductCommand), result.CommandType);
            Assert.AreEqual(true, result.IsAsyncMethod);
        }

        [TestMethod]
        public void SetLoggerFactory_ShouldSetTheLoggerFactoryForTheProject()
        {
            var factory = new LoggerFactory();
            _target.SetLoggerFactory(factory);
            Assert.AreEqual(NijnWebScaleLogger.LoggerFactory, factory);
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
            Assert.AreEqual(1, result.EventListeners.Count);
            Assert.AreEqual(TestClassesConstants.ProductEventListenerQueueName, listener.QueueName);
            Assert.AreEqual(TestClassesConstants.ProductEventHandlerTopic, listener.TopicExpressions.First());
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWithOneCommandListenerWhenCalledWithAddEventListener()
        {
            var result = _target.AddListener<OrderCommandListener>().WithContext(_busContextMock.Object).CreateHost();

            var listener = result.CommandListeners.First();
            Assert.AreEqual(1, result.CommandListeners.Count);
            Assert.AreEqual(TestClassesConstants.OrderCommandListenerQueueName, listener.QueueName);
        }

        [TestMethod]
        public void CreateHost_ShouldReturnMicroserviceHostWhenCalledWithUseConventions()
        {
            var result = _target.UseConventions().WithContext(_busContextMock.Object).CreateHost();

            Assert.AreEqual(2, result.CommandListeners.Count);
            Assert.AreEqual(2, result.EventListeners.Count);
        }

        [TestMethod]
        public void CreateHost_ShouldSetTheExceptionDictionaryInTheCommandPublisher()
        {
            _target.ScanForExceptions().WithContext(_busContextMock.Object).CreateHost();
            Assert.AreEqual(3, CommandPublisher.ExceptionTypes.Count);
        }
    }
}
