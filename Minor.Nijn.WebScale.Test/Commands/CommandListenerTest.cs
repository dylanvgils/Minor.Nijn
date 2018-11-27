using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses;
using Moq;
using RabbitMQ.Client;
using System;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Newtonsoft.Json;

namespace Minor.Nijn.WebScale.Commands.Test
{
    [TestClass]
    public class CommandListenerTest
    {
        private Type type;
        private string queueName;

        private CommandListener target;

        [TestInitialize]
        public void Ctor_ShouldCreateNewCommandListener()
        {
            type = typeof(OrderEventListener);
            var method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);
            queueName = "queueName";

            target = new CommandListener(type, method, queueName);
        }

        [TestMethod]
        public void Ctor_ShouldCreatedNewEventListener()
        {
            var type = typeof(OrderEventListener);
            var method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);
            var queueName = "queueName";

            var listener = new CommandListener(type, method, queueName);

            Assert.AreEqual(queueName, listener.QueueName);
        }

        [TestMethod]
        public void StartListening_ShouldStartListeningForCommand()
        {
            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);

            target.StartListening(hostMock.Object);

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
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);

            target.StartListening(hostMock.Object);
            Action action = () => { target.StartListening(hostMock.Object); };

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();

            var ex =Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("Already listening for commands", ex.Message);
        }

        [TestMethod]
        [Ignore]
        public void HandleCommandMessage_ShouldHandleCommandMessageAndReturnCommandResponse()
        {
            var correlationId = "correlationId";
            var order = new Order { Id = 1, Description = "Some description" };
            var commandMessage = new RequestCommandMessage(JsonConvert.SerializeObject(order), "Order", correlationId, queueName);

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
        }

        [TestMethod]
        public void Dispose_ShouldDisposeResources()
        {
            var commandReceiverMock = new Mock<ICommandReceiver>(MockBehavior.Strict);
            commandReceiverMock.Setup(recv => recv.DeclareCommandQueue());
            commandReceiverMock.Setup(recv => recv.StartReceivingCommands(It.IsAny<CommandReceivedCallback>()));
            commandReceiverMock.Setup(recv => recv.Dispose());

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateCommandReceiver(queueName)).Returns(commandReceiverMock.Object);

            var hostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            hostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));
            hostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);

            target.StartListening(hostMock.Object);
            target.Dispose();

            commandReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
            hostMock.VerifyAll();

            hostMock.VerifyAll();
        }
    }
}