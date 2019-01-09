using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.Helpers;
using Moq;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace Minor.Nijn.RabbitMQBus.Test
{
    [TestClass]
    public class RabbitMQBusContextBuilderTest
    {
        [TestInitialize]
        public void BeforeEach()
        {
            Environment.SetEnvironmentVariable(Constants.EnvExchangeName, "exchange");
            Environment.SetEnvironmentVariable(Constants.EnvHostname, "hostname");
            Environment.SetEnvironmentVariable(Constants.EnvPort, "1024");
            Environment.SetEnvironmentVariable(Constants.EnvUsername, "username");
            Environment.SetEnvironmentVariable(Constants.EnvPassword, "password");
            Environment.SetEnvironmentVariable(Constants.EnvExchangeType, "type");
        }

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
        public void ContextHasRightIdleAfter()
        {
            var connectionBuilder = new RabbitMQContextBuilder();

            connectionBuilder.WithConnectionTimeout(5000, true);

            Assert.AreEqual(5000, connectionBuilder.ConnectionTimeoutAfterMs);
            Assert.IsTrue(connectionBuilder.AutoDisconnectEnabled, "AutoDisconnectEnabled should be true");
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
                .WithConnectionTimeout(2000)
                .CreateContext();
            
            factoryMock.VerifyAll();
            connectionMock.VerifyAll();
            modelMock.VerifyAll();

            Assert.IsInstanceOfType(result, typeof(RabbitMQBusContext));
            Assert.AreEqual("ExchangeName", result.ExchangeName);
            Assert.AreEqual(2000, result.ConnectionTimeoutMs);
            Assert.IsFalse(result.AutoDisconnectEnabled, "AutoDisconnectEnabled should be false");
        }

        [TestMethod]
        public void CreateContext_ShouldThrowExceptionWhenUnableToConnectToTheRabbitMQHost()
        {
            var factoryMock = new Mock<IConnectionFactory>(MockBehavior.Strict);
            factoryMock.Setup(fact => fact.CreateConnection())
                .Throws(new BrokerUnreachableException(new Exception()));

            var target = new RabbitMQContextBuilder(factoryMock.Object)
                .WithExchange("ExchangeName")
                .WithAddress(hostName: "localhost", port: 5642)
                .WithCredentials(userName: "username", password: "password");

            Action action = () => { target.CreateContext(); };

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual("Unable to connect to the RabbitMQ host", ex.Message);
        }

        [TestMethod]
        public void CreateContext_ShouldThrowExceptionWhenCalledFromExtensionMethod()
        {
            var loggerFactory = new LoggerFactory();

            var services = new ServiceCollection();

            Action action = () =>
            {
                services.AddNijn(options =>
                {
                    options
                        .SetLoggerFactory(loggerFactory)
                        .CreateContext();
                });
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(action);
            Assert.AreEqual("CreateContext is not allowed in AddNijn extension method", ex.Message);
        }

        [TestMethod]
        public void SetLoggerFactory_ShouldSetTheLoggerFactoryForTheProject()
        {
            var factory = new LoggerFactory();
            new RabbitMQContextBuilder().SetLoggerFactory(factory);
            Assert.AreEqual(NijnLogger.LoggerFactory, factory);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_ShouldLoadEnvironmentVariables()
        {
            var result = new RabbitMQContextBuilder().ReadFromEnvironmentVariables();

            Assert.AreEqual("exchange", result.ExchangeName);
            Assert.AreEqual("hostname", result.Hostname);
            Assert.AreEqual(1024, result.Port);
            Assert.AreEqual("username", result.Username);
            Assert.AreEqual("password", result.Password);
            Assert.AreEqual("type", result.Type);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_ShouldUseDefaultValueWhenSet()
        {
            Environment.SetEnvironmentVariable(Constants.EnvExchangeType, null);
            var result = new RabbitMQContextBuilder().ReadFromEnvironmentVariables();
            Assert.AreEqual("topic", result.Type);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_ShouldThrowExceptionWhenEnvironmentVariableIsNotSet()
        {
            Environment.SetEnvironmentVariable(Constants.EnvHostname, null);
            Action action = () => { new RabbitMQContextBuilder().ReadFromEnvironmentVariables(); };
            Assert.ThrowsException<ArgumentException>(action);
        }

        [TestMethod]
        public void ReadFromEnvironmentVariables_ShouldThrowExceptionWhenUnableToParsePort()
        {
            Environment.SetEnvironmentVariable(Constants.EnvPort, "abc");
            Action action = () => { new RabbitMQContextBuilder().ReadFromEnvironmentVariables(); };
            var ex = Assert.ThrowsException<ArgumentException>(action);
            Assert.AreEqual($"Invalid environment variable: {Constants.EnvPort}, could not parse value to int", ex.Message);
        }

        [TestMethod]
        public void CreateContextWithRetry_ShouldRetryToConnectWhenConnectingFailed()
        {
            var modelMock = new Mock<IModel>(MockBehavior.Strict);
            modelMock.Setup(chan => chan.Dispose());
            modelMock.Setup(chan => chan.ExchangeDeclare("exchange", "type", false, false, null));

            var connectionMock = new Mock<IConnection>(MockBehavior.Strict);
            connectionMock.Setup(conn => conn.CreateModel()).Returns(modelMock.Object);

            var factoryMock = new Mock<IConnectionFactory>(MockBehavior.Strict);
            factoryMock.SetupSequence(fact => fact.CreateConnection())
                .Throws(new BrokerUnreachableException(new Exception()))
                .Returns(connectionMock.Object);

            var result = new RabbitMQContextBuilder(factoryMock.Object)
                .ReadFromEnvironmentVariables()
                .CreateContextWithRetry(3, 500);

            factoryMock.VerifyAll();
            connectionMock.VerifyAll();
            modelMock.VerifyAll();

            Assert.IsInstanceOfType(result, typeof(RabbitMQBusContext));
        }

        [TestMethod]
        public void CreateContextWithRetry_ShouldThrowExceptionWhenConnectingFailed()
        {
            var factoryMock = new Mock<IConnectionFactory>(MockBehavior.Strict);
            factoryMock.Setup(fact => fact.CreateConnection()).Throws(new BrokerUnreachableException(new Exception()));

            var target = new RabbitMQContextBuilder(factoryMock.Object).ReadFromEnvironmentVariables();

            Action action = () => { target.CreateContextWithRetry(3, 500); };

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            factoryMock.Verify(fact => fact.CreateConnection(), Times.Exactly(3));
            Assert.AreEqual("Connecting to the RabbitMQ host failed", ex.Message);
        }
    }
}