using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.InvalidTestClasses;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Injectable;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using ProductCommandListener = Minor.Nijn.WebScale.Test.TestClasses.ProductCommandListener;

namespace Minor.Nijn.WebScale.Commands.Test
{
    [TestClass]
    public class CommandListenerTest
    {
        private Type _type;
        private string _queueName;

        private CommandListener _target;

        [TestInitialize]
        public void BeforeEach()
        {
            _type = typeof(OrderCommandListener);
            var method = _type.GetMethod(TestClassesConstants.OrderCommandHandlerMethodName);
            _queueName = "queueName";

            var meta = new CommandListenerInfo
            {
                QueueName = _queueName,
                Type = _type,
                Method = method,
                IsAsyncMethod = false,
                CommandType = method.GetParameters()[0].ParameterType
            };

            _target = new CommandListener(meta);
        }

        [TestCleanup]
        public void AfterEach()
        {
            OrderCommandListener.HandleOrderCreatedEventHasBeenCalled = false;
            OrderCommandListener.HandleOrderCreatedEventHasBeenCalledWith = null;
        }

        [TestMethod]
        public void Ctor_ShouldCreateNewCommandListener()
        {
            var type = typeof(OrderEventListener);
            var method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);
            var queueName = "queueName";

            var meta = new CommandListenerInfo
            {
                QueueName = queueName,
                Type = type,
                Method = method,
                IsAsyncMethod = false,
                CommandType = method.GetParameters()[0].ParameterType
            };

            var listener = new CommandListener(meta);

            Assert.AreEqual(queueName, listener.QueueName);
        }

        [TestMethod]
        public void StartListening_ShouldStartListeningForCommand()
        {
            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(_queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(_type)).Returns(Activator.CreateInstance(_type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);

            _target.StartListening(hostMock.Object);

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();
        }

        [TestMethod]
        public void StartListening_ShouldThrowExceptionWhenAlreadyListening()
        {
            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(_queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(_type)).Returns(Activator.CreateInstance(_type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);

            _target.StartListening(hostMock.Object);
            Action action = () => { _target.StartListening(hostMock.Object); };

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();

            var ex =Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("Already listening for commands", ex.Message);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void StartListening_ShouldThrowExceptionWhenDisposed()
        {
            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            _target.Dispose();
            _target.StartListening(hostMock.Object);
        }

        [TestMethod]
        public void HandleCommandMessage_ShouldHandleRequestCommandMessageAndReturnTheRightResult()
        {
            var order = new Order { Id = 1, Description = "Some description" };
            var command = new AddOrderCommand(_queueName, order);

            var commandMessage = new RequestCommandMessage(
                message: JsonConvert.SerializeObject(command), 
                type: command.GetType().Name, 
                correlationId: command.CorrelationId, 
                routingKey: _queueName, 
                timestamp: command.Timestamp
            );

            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(_queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(_type)).Returns(Activator.CreateInstance(_type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            _target.StartListening(hostMock.Object);

            OrderCommandListener.ReplyWith = order;
            var response = _target.HandleCommandMessage(commandMessage);

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();

            var request = OrderCommandListener.HandleOrderCreatedEventHasBeenCalledWith;
            Assert.IsTrue(OrderCommandListener.HandleOrderCreatedEventHasBeenCalled);

            Assert.IsNotNull(response, "Response command should not be null");
            Assert.AreEqual(command.CorrelationId, response.CorrelationId);
            Assert.AreEqual(JsonConvert.SerializeObject(order), response.Message);

            Assert.IsNotNull(request, "Request command should not be null");
            Assert.AreEqual(_queueName, request.RoutingKey);
            Assert.AreEqual(command.CorrelationId, request.CorrelationId);
            Assert.AreEqual(command.Timestamp, request.Timestamp);
            Assert.AreEqual(order.Id, request.Order.Id);
            Assert.AreEqual(order.Description, request.Order.Description);
        }

        [TestMethod]
        public void HandleCommandMessage_ShouldHandleAsyncCommandListeners()
        {
            var command = new AddProductCommand(_queueName, 42);

            var commandMessage = new RequestCommandMessage(
                message: JsonConvert.SerializeObject(command),
                type: command.GetType().Name,
                correlationId: command.CorrelationId,
                routingKey: _queueName,
                timestamp: command.Timestamp
            );


            var type = typeof(ProductCommandListener);
            var method = type.GetMethod(TestClassesConstants.ProductCommandHandlerMethodName);
            var meta = new CommandListenerInfo
            {
                QueueName = _queueName,
                Type = type,
                Method = method,
                IsAsyncMethod = true,
                CommandType = method.GetParameters()[0].ParameterType
            };

            var target = new CommandListener(meta);

            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(_queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type, new object[] { new Foo() }));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            target.StartListening(hostMock.Object);

            var response = target.HandleCommandMessage(commandMessage);

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();

            Assert.AreEqual("42", response.Message);
        }

        [TestMethod]
        public void HandleCommandMessage_ShouldHandleRequestCommandWithWrongType()
        {
            var order = new Order { Id = 1, Description = "Some description" };
            var command = new AddOrderCommand(_queueName, order);

            var commandMessage = new RequestCommandMessage(
                message: JsonConvert.SerializeObject(command),
                type: "WrongType",
                correlationId: command.CorrelationId,
                routingKey: _queueName,
                timestamp: command.Timestamp
            );

            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(_queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(_type)).Returns(Activator.CreateInstance(_type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            _target.StartListening(hostMock.Object);

            var result = _target.HandleCommandMessage(commandMessage);

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();

            Assert.IsFalse(OrderCommandListener.HandleOrderCreatedEventHasBeenCalled);
            Assert.AreEqual("ArgumentException", result.Type);
            Assert.AreEqual(command.CorrelationId, result.CorrelationId);
        }

        [TestMethod]
        public void HandleCommandMessage_ShouldHandleExceptionThrownByCommandListenerMethod()
        {
            var queueName = "InvalidCommandQueue";
            var command = new AddProductCommand(queueName, 42);

            var commandMessage = new RequestCommandMessage(
                message: JsonConvert.SerializeObject(command),
                type: command.GetType().Name,
                correlationId: command.CorrelationId,
                routingKey: queueName,
                timestamp: command.Timestamp
            );

            var type = typeof(InvalidCommandListenerException);
            var method = type.GetMethod("ThrowException");
            var meta = new CommandListenerInfo
            {
                QueueName = queueName,
                Type = type,
                Method = method,
                IsAsyncMethod = false,
                CommandType = method.GetParameters()[0].ParameterType
            };

            var target = new CommandListener(meta);

            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);

            target.StartListening(hostMock.Object);

            var result = target.HandleCommandMessage(commandMessage);

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();

            Assert.AreEqual("NullReferenceException", result.Type);
            Assert.AreEqual(command.CorrelationId, result.CorrelationId);
        }

        [TestMethod]
        public void Dispose_ShouldDisposeResources()
        {
            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));
            commandReceiverMock.Setup(recv => recv.Dispose());

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(_queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(_type)).Returns(Activator.CreateInstance(_type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);

            _target.StartListening(hostMock.Object);
            _target.Dispose();
            _target.Dispose(); // Don't call dispose the second time

            commandReceiverMock.Verify(c => c.Dispose(), Times.Once);
        }
    }
}