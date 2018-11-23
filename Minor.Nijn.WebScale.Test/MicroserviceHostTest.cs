using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Reflection;
using Minor.Nijn.WebScale.Test.TestClasses;

namespace Minor.Nijn.WebScale.Test
{
    [TestClass]
    public class MicroserviceHostTest
    {
        private Type type;
        private MethodInfo method;
        private Mock<IBusContext<IConnection>> busContextMock;

        private MicroserviceHost target;

        [TestInitialize]
        public void BeforeEach()
        {
            type = typeof(OrderEventListener);
            method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);

            var listeners = new List<EventListener>
            {
                new EventListener(type, method, "QueueName", new List<string> { "a.b.c" })
            };

            busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);

            target = new MicroserviceHost(busContextMock.Object, listeners);
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldCreateMessageReceivers()
        {
            var receiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            receiverMock.Setup(recv => recv.DeclareQueue());
            receiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            busContextMock.Setup(ctx => ctx.CreateMessageReceiver("QueueName", new List<string> {"a.b.c"}))
                .Returns(receiverMock.Object);

            target.RegisterEventListeners();

            busContextMock.VerifyAll();
            receiverMock.VerifyAll();
        }

        [TestMethod]
        public void RegisterEventListeners_ShouldThrowExceptionWhenCalledForTheSecondTime()
        {
            var receiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            receiverMock.Setup(recv => recv.DeclareQueue());
            receiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            busContextMock.Setup(ctx => ctx.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<List<string>>()))
                .Returns(receiverMock.Object);

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
            busContextMock.Setup(ctx => ctx.Connection.Dispose());
            target.Dispose();
            busContextMock.VerifyAll();
        }
    }
}
