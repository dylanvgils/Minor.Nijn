using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Moq;
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

            var senderMock = new Mock<IMessageSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendMessage(It.IsAny<EventMessage>()));

            var busContextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);;
            busContextMock.Setup(ctx => ctx.CreateMessageSender()).Returns(senderMock.Object);

            var target = new EventPublisher(busContextMock.Object);
            target.Publish(orderCreatedEvent);

            senderMock.VerifyAll();
            busContextMock.VerifyAll();
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

            senderMock.VerifyAll();
            busContextMock.VerifyAll();
        }
    }
}
