using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQbusContextTest
    {
        private string exchangeName = "ExchangeName";

        private Mock<IConnection> connectionMock;
        private Mock<IModel> channelMock;

        private RabbitMQBusContext target;

        [TestInitialize]
        public void BeforeEach()
        {
            connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            channelMock = new Mock<IModel>(MockBehavior.Strict);

            connectionMock.Setup(conn => conn.CreateModel()).Returns(channelMock.Object);

            target = new RabbitMQBusContext(connectionMock.Object, exchangeName);
        }
        
        [TestMethod]
        public void CreateMessageSender_ShouldReturnIMessageSender()
        {            
            var result = target.CreateMessageSender();
            
            connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(IMessageSender));
        }
        
        [TestMethod]
        public void CreateMessageReceiver_ShouldReturnIMessageReceiver()
        {
            var queueName = "queueName";
            IEnumerable<string> topicExpressions = new List<string> { "a.b.c" };
            
            var result = target.CreateMessageReceiver(queueName, topicExpressions);
            
            connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(IMessageReceiver));
        }
        
        [TestMethod]
        public void CreateCommandSender_ShouldReturnICommandSender()
        {
            var result = target.CreateCommandSender();

            connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(ICommandSender));
        }
        
        [TestMethod]
        public void CreateCommandReceiver_ShouldReturnICommandReceiver()
        {
            var result = target.CreateCommandReceiver();

            connectionMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(ICommandReceiver));
        }
    }
}
