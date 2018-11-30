using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Minor.Nijn.WebScale.Events.Test
{
    [TestClass]
    public class EventPublisherTest
    {
        [TestMethod]
        public void Publish_ShouldSendEventMessage()
        {
            var orderCreatedEvent = new OrderCreatedEvent("test", new Order { Id = 1, Description = "test" });

            EventMessage result = null;
            var senderMock = new Mock<IMessageSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendMessage(It.IsAny<EventMessage>()))
                .Callback((EventMessage m) => result = m); ;

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);;
            busContextMock.Setup(ctx => ctx.CreateMessageSender()).Returns(senderMock.Object);

            var target = new EventPublisher(busContextMock.Object);
            target.Publish(orderCreatedEvent);

            senderMock.VerifyAll();
            busContextMock.VerifyAll();

            Assert.AreEqual(orderCreatedEvent.RoutingKey, result.RoutingKey);
            Assert.AreEqual(orderCreatedEvent.CorrelationId, result.CorrelationId);
            Assert.AreEqual(orderCreatedEvent.Timestamp, result.Timestamp);
            Assert.AreEqual(orderCreatedEvent.GetType().Name, result.Type);
            Assert.AreEqual(JsonConvert.SerializeObject(orderCreatedEvent), result.Message);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public void Publish_ShouldThrowExceptionWhenDisposed()
        {
            var commandSenderMock = new Mock<IMessageSender>(MockBehavior.Strict);
            commandSenderMock.Setup(s => s.Dispose());

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateMessageSender()).Returns(commandSenderMock.Object);

            var message = new OrderCreatedEvent("RoutinKey", new Order());
            var target = new EventPublisher(contextMock.Object);

            target.Dispose();
            target.Publish(message);
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            var senderMock = new Mock<IMessageSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.Dispose());

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict); ;
            busContextMock.Setup(ctx => ctx.CreateMessageSender()).Returns(senderMock.Object);

            var target = new EventPublisher(busContextMock.Object);
            target.Dispose();
            target.Dispose(); // Don't call dispose the second time

            senderMock.Verify(s => s.Dispose(), Times.Once());
        }
    }
}
