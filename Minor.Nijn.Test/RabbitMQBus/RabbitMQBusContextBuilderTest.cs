using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RabbitMQ.Client;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQBusContextBuilderTest
    {
        [TestMethod]
        public void ContextHasRightExchangeName()
        {
            var connectionBuilder = new RabbitMQContextBuilder();

            connectionBuilder.WithExchange("MVM.EventExchange");
               
            Assert.AreEqual("MVM.EventExchange", connectionBuilder.ExchangeName);
        }

        [TestMethod]
        public void ContextHasRightAddress()
        {
            var connectionBuilder = new RabbitMQContextBuilder();

            connectionBuilder.WithAddress("localhost", 1234);

            Assert.AreEqual("localhost", connectionBuilder.Hostname);
            Assert.AreEqual(1234, connectionBuilder.Port);
        }

        [TestMethod]
        public void ContextHasRightCredentials()
        {
            var connectionBuilder = new RabbitMQContextBuilder();

            connectionBuilder.WithCredentials(userName: "guest", password: "password");

            Assert.AreEqual("guest", connectionBuilder.Username);
            Assert.AreEqual("password", connectionBuilder.Password);
        }
        
        [TestMethod]
        public void BuildingWithMethodChainingWorks()
        {
            var connectionBuilder = new RabbitMQContextBuilder();

            connectionBuilder.WithExchange("MVM.EventExchange")
                .WithAddress("localhost", 1234)
                .WithCredentials(userName: "guest", password: "password");

            Assert.AreEqual("MVM.EventExchange", connectionBuilder.ExchangeName);

            Assert.AreEqual("localhost", connectionBuilder.Hostname);
            Assert.AreEqual(1234, connectionBuilder.Port);

            Assert.AreEqual("guest", connectionBuilder.Username);
            Assert.AreEqual("password", connectionBuilder.Password);
        }
        
        [TestMethod]
        public void CreateContext_ShouldReturnRabbitMQContext()
        {            
            var modelMock = new Mock<IModel>(MockBehavior.Strict);
            modelMock.Setup(chan => chan.Dispose());
            modelMock.Setup(chan => chan.ExchangeDeclare("ExchangeName", "type", false, false, null));
            
            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.CreateModel()).Returns(modelMock.Object);
            
            var factoryMock = new Mock<IConnectionFactory>(MockBehavior.Strict);
            factoryMock.Setup(fact => fact.CreateConnection()).Returns(connectionMock.Object);
            
            var result = new RabbitMQContextBuilder(factoryMock.Object)
                .WithExchange("ExchangeName")
                .WithAddress(hostName: "localhost", port: 5642)
                .WithCredentials(userName: "username", password: "password")
                .WithType("type")
                .CreateContext();
            
            factoryMock.VerifyAll();
            connectionMock.VerifyAll();
            modelMock.VerifyAll();
            Assert.IsInstanceOfType(result, typeof(RabbitMQBusContext));
        }

        [TestMethod]
        public void SetLoggerFactory_ShouldSetTheLoggerFactoryForTheProject()
        {
            var factory = new LoggerFactory();
            new RabbitMQContextBuilder().SetLoggerFactory(factory);
            Assert.AreEqual(NijnLogger.LoggerFactory, factory);
        }
    }
}