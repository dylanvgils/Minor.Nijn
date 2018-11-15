using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class EventingBasicConsumerFactoryTest
    {
        [TestMethod]
        public void ShouldCreateEventingBasicConsumer()
        {
            var mock = new Mock<IModel>(MockBehavior.Strict);
            var target = new EventingBasicConsumerFactory();
            
            var result = target.CreateEventingBasicConsumer(mock.Object);
            
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(EventingBasicConsumer));
        }
    }
}