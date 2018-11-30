﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;

namespace Minor.Nijn.WebScale.Events.Test
{
    [TestClass]
    public class EventListenerTest
    {
        private Type type;
        private string queueName;
        private IEnumerable<string> topicExpressions;

        private EventListener target;

        [TestInitialize]
        public void BeforeEach()
        {
            type = typeof(OrderEventListener);
            var method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);
            queueName = "queueName";
            topicExpressions = new List<string> { "a.b.c" };

            target = new EventListener(type, method, queueName, topicExpressions);
        }

        [TestCleanup]
        public void AfterEach()
        {
            OrderEventListener.HandleOrderCreatedEventHasBeenCalled = false;
            OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith = null;
        }

        [TestMethod]
        public void Ctor_ShouldCreatedNewEventListener()
        {
            var type = typeof(OrderEventListener);
            var method = type.GetMethod(TestClassesConstants.OrderEventHandlerMethodName);
            var queueName = "queueName";
            var topicExpressions = new List<string> {"a.b.c"};

            var listener = new EventListener(type, method, queueName, topicExpressions);

            Assert.AreEqual(queueName, listener.QueueName);
            Assert.AreEqual(topicExpressions, listener.TopicExpressions);
        }

        [TestMethod]
        public void StartListening_ShouldStartListeningForMessages()
        {
            var messageReceiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            messageReceiverMock.Setup(recv => recv.DeclareQueue());
            messageReceiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateMessageReceiver(queueName, topicExpressions))
                .Returns(messageReceiverMock.Object);

            var microServiceHostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            microServiceHostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            microServiceHostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));

            target.StartListening(microServiceHostMock.Object);

            microServiceHostMock.VerifyAll();
            messageReceiverMock.VerifyAll();
            busContextMock.VerifyAll();
        }

        [TestMethod]
        public void StartListening_ShouldThrowInvalidOperationExceptionWhenCalledForTheSecondTime()
        {
            var messageReceiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            messageReceiverMock.Setup(recv => recv.DeclareQueue());
            messageReceiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateMessageReceiver(queueName, topicExpressions))
                .Returns(messageReceiverMock.Object);

            var microServiceHostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            microServiceHostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            microServiceHostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));

            target.StartListening(microServiceHostMock.Object);
            Action action = () => { target.StartListening(microServiceHostMock.Object); };
        
            microServiceHostMock.VerifyAll();
            messageReceiverMock.VerifyAll();
            busContextMock.VerifyAll();

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("Already listening for events", ex.Message);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void StartListening_ShouldThrowExceptionWhenDisposed()
        {
            var microServiceHostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            target.Dispose();
            target.StartListening(microServiceHostMock.Object);
        }

        [TestMethod]
        public void HandleEventMessage_ShouldHandleEventMessageAndReturnCastedType()
        {
            var routingKey = "a.b.c";
            var order = new Order { Id = 1, Description = "Some Description" };
            var orderCreatedEvent = new OrderCreatedEvent(routingKey, order);
            var eventMessage = new EventMessage(
                routingKey: routingKey, 
                message: JsonConvert.SerializeObject(orderCreatedEvent),
                type: orderCreatedEvent.GetType().Name,
                timestamp: orderCreatedEvent.Timestamp,
                correlationId: orderCreatedEvent.CorrelationId
            );

            var messageReceiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            messageReceiverMock.Setup(recv => recv.DeclareQueue());
            messageReceiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateMessageReceiver(queueName, topicExpressions))
                .Returns(messageReceiverMock.Object);

            var microServiceHostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            microServiceHostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            microServiceHostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));
            target.StartListening(microServiceHostMock.Object);

            target.HandleEventMessage(eventMessage);

            var result = OrderEventListener.HandleOrderCreatedEventHasBeenCalledWith;
            Assert.IsTrue(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);
            Assert.AreEqual(orderCreatedEvent.RoutingKey, result.RoutingKey);
            Assert.AreEqual(orderCreatedEvent.CorrelationId, result.CorrelationId);
            Assert.AreEqual(orderCreatedEvent.Timestamp, result.Timestamp);
            Assert.AreEqual(order.Id, result.Order.Id);
            Assert.AreEqual(order.Description, result.Order.Description);
        }

        [TestMethod]
        public void HandleEventMessage_ShouldHandleEventMessageWithWrongType()
        {
            var routingKey = "a.b.c";
            var order = new Order { Id = 1, Description = "Some Description" };
            var orderCreatedEvent = new OrderCreatedEvent(routingKey, order);
            var eventMessage = new EventMessage(
                routingKey: routingKey,
                message: JsonConvert.SerializeObject(orderCreatedEvent),
                type: "WrongType",
                timestamp: orderCreatedEvent.Timestamp,
                correlationId: orderCreatedEvent.CorrelationId
            );

            var messageReceiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            messageReceiverMock.Setup(recv => recv.DeclareQueue());
            messageReceiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateMessageReceiver(queueName, topicExpressions))
                .Returns(messageReceiverMock.Object);

            var microServiceHostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            microServiceHostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            microServiceHostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));
            target.StartListening(microServiceHostMock.Object);

            target.HandleEventMessage(eventMessage);

            Assert.IsFalse(OrderEventListener.HandleOrderCreatedEventHasBeenCalled);
        }

        [TestMethod]
        public void Dispose_ShouldDisposeResources()
        {
            var messageReceiverMock = new Mock<IMessageReceiver>(MockBehavior.Strict);
            messageReceiverMock.Setup(recv => recv.DeclareQueue());
            messageReceiverMock.Setup(recv => recv.StartReceivingMessages(It.IsAny<EventMessageReceivedCallback>()));
            messageReceiverMock.Setup(recv => recv.Dispose());

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            busContextMock.Setup(ctx => ctx.CreateMessageReceiver(It.IsAny<string>(), It.IsAny<List<string>>())).Returns(messageReceiverMock.Object);

            var microServiceHostMock = new Mock<IMicroserviceHost>(MockBehavior.Strict);
            microServiceHostMock.SetupGet(host => host.Context).Returns(busContextMock.Object);
            microServiceHostMock.Setup(host => host.CreateInstance(type)).Returns(Activator.CreateInstance(type));

            target.StartListening(microServiceHostMock.Object);
            target.Dispose();
            target.Dispose(); // Don't call dispose the second time

            messageReceiverMock.Verify(recv => recv.Dispose(), Times.Once);
        }
    }
}
