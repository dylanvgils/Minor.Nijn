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
using Microsoft.Extensions.DependencyInjection;
using Minor.Nijn.WebScale.Helpers;
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
            Assert.AreEqual(3, result.Count());
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
        public void AddListener_ShouldThrowArgumentExceptionWhenAsyncCommandListenerMethodHasInvalidReturnType()
        {
            Action action = () => { _target.AddListener<InvalidCommandListenerAsyncReturnType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid return type of method: InvalidAsyncReturnType, return type of async CommandListener method should be of type Task<T>", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenCommandListenerHasParamTypeEventMessage()
        {
            Action action = () => { _target.AddListener<InvalidCommandListenerEventMessageParamType>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action); 
            Assert.AreEqual("Invalid parameter type in method: 'ParameterTypeEventMessageInvalid', parameter has to be derived type of DomainCommand", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldThrowArgumentExceptionWhenListenerMethodHasNoParameters()
        {
            Action action = () => { _target.AddListener<InvalidParamTypeNone>(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid parameter type in method: 'ParamTypeInvalidNone', parameter has to be derived type of DomainEvent", ex.Message);
        }

        [TestMethod]
        public void AddListener_ShouldPassCorrectEventListenerInfoToEventListener()
        {
            _target.AddListener<OrderEventListener>();

            var result = _target.EventListeners.First().Meta;
            Assert.AreEqual(TestClassesConstants.OrderEventListenerQueueName, result.QueueName);
            Assert.AreEqual(typeof(OrderEventListener), result.Type);
            Assert.IsFalse(result.IsSingleton, "Listener should not be singleton");

            var methodInfo = result.Methods.First();
            Assert.AreEqual(TestClassesConstants.OrderEventHandlerTopic, methodInfo.TopicExpressions.First());
            Assert.AreEqual(TestClassesConstants.OrderEventHandlerMethodName, methodInfo.Method.Name);
            Assert.AreEqual(typeof(OrderCreatedEvent), methodInfo.EventType);
            Assert.AreEqual(false, methodInfo.IsAsync);
        }

        [TestMethod]
        public void AddListener_ShouldPassCorrectAsyncEventListenerInfoToEventListener()
        {
            _target.AddListener<ProductEventListener>();

            var result = _target.EventListeners.First().Meta;
            Assert.AreEqual(TestClassesConstants.ProductEventListenerQueueName, result.QueueName);
            Assert.AreEqual(typeof(ProductEventListener), result.Type);
            Assert.IsFalse(result.IsSingleton, "Listener should not be singleton");

            var methodInfo = result.Methods.First();
            Assert.AreEqual(TestClassesConstants.ProductEventHandlerTopic, methodInfo.TopicExpressions.First());
            Assert.AreEqual(TestClassesConstants.ProductEventHandlerMethodName, methodInfo.Method.Name);
            Assert.AreEqual(typeof(ProductAddedEvent), methodInfo.EventType);
            Assert.AreEqual(true, methodInfo.IsAsync);
        }

        [TestMethod]
        public void AddListener_ShouldPassCorrectCommandListenerInfoToCommandListener()
        {
            _target.AddListener<OrderCommandListener>();

            var result = _target.CommandListeners.First().Meta;
            Assert.AreEqual(TestClassesConstants.OrderCommandListenerQueueName, result.QueueName);
            Assert.AreEqual(typeof(OrderCommandListener), result.Type);
            Assert.IsFalse(result.IsSingleton, "Listener should not be singleton");
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
            Assert.IsFalse(result.IsSingleton, "Listener should not be singleton");
            Assert.AreEqual(TestClassesConstants.ProductCommandHandlerMethodName, result.Method.Name);
            Assert.AreEqual(typeof(AddProductCommand), result.CommandType);
            Assert.AreEqual(true, result.IsAsyncMethod);
        }


        [TestMethod]
        public void AddListener_ShouldThrowExceptionWhenThereIsAlreadyAListenerDeclaredWithTheSameName()
        {
            _target.AddListener<OrderEventListener>();

            Action action = () => { _target.AddListener<OrderEventListener>(); };

            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual("Invalid queue name: EventBus.OrderEventQueue, there is already declared a listener with the same name", ex.Message);
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
            Assert.AreEqual(3, result.EventListeners.Count);
        }

        [TestMethod]
        public void CreateHost_ShouldSetTheExceptionDictionaryInTheCommandPublisher()
        {
            _target.ScanForExceptions().WithContext(_busContextMock.Object).CreateHost();
            Assert.AreEqual(3, CommandPublisher.ExceptionTypes.Count);
        }

        [TestMethod]
        public void CreateHost_ShouldThrowExceptionWhenCalledFromExtensionMethod()
        {
            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            var services = new ServiceCollection();

            Action action = () =>
            {
                services.AddNijnWebScale(options => { options.WithContext(contextMock.Object).CreateHost(); });
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("CreateHost is not allowed in AddNijnWebScale extension method", ex.Message);
        }

        [TestMethod]
        public void CreateHost_ShouldThrowExceptionWhenContextIsNull()
        {
            Action action = () => { _target.CreateHost(); };
            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("MicroserviceHost can not be created without context", ex.Message);
        }
    }
}
