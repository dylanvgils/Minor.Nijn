using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.WebScale.Test.InvalidTestClasses;
using Minor.Nijn.WebScale.Test.TestClasses.Commands;
using Minor.Nijn.WebScale.Test.TestClasses.Domain;
using Minor.Nijn.WebScale.TestClasses.Exceptions.Test;
using Moq;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Minor.Nijn.WebScale.Commands.Test
{
    [TestClass]
    public class CommandPublisherTest
    {
        [TestCleanup]
        public void AfterEach()
        {
            CommandPublisher.ExceptionTypes = null;
        }

        [TestMethod]
        public void Publish_ShouldSendCommandAndReturnResult()
        {
            var input = 21;
            var command = new AddProductCommand("RoutingKey", input);

            CommandMessage request = null;
            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendCommandAsync(It.IsAny<RequestCommandMessage>()))
                .ReturnsAsync(new ResponseCommandMessage(JsonConvert.SerializeObject(input * 2), "int", "correlationId"))
                .Callback((RequestCommandMessage m) => request = m);

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);

            var target = new CommandPublisher(contextMock.Object);
            var result = target.Publish<int>(command);

            Assert.AreEqual(command.RoutingKey, request.RoutingKey);
            Assert.AreEqual(command.CorrelationId, request.CorrelationId);
            Assert.AreEqual(command.Timestamp, request.Timestamp);
            Assert.AreEqual(JsonConvert.SerializeObject(command), request.Message);

            Assert.AreEqual(42, result.Result);
        }

        [TestMethod, ExpectedException(typeof(ArgumentException))]
        public async Task Publish_ShouldReThrowArgumentException()
        {
            var requestCommand = new AddProductCommand("RoutingKey", 42);
            var exception = new ArgumentException("Some exception message");

            var responseCommand = new ResponseCommandMessage(
                message: JsonConvert.SerializeObject(exception),
                type: exception.GetType().Name,
                correlationId: requestCommand.CorrelationId,
                timestamp: requestCommand.Timestamp
            );

            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendCommandAsync(It.IsAny<RequestCommandMessage>()))
                .ReturnsAsync(responseCommand);

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);

            var target = new CommandPublisher(contextMock.Object);

            await target.Publish<int>(requestCommand);
        }

        [TestMethod, ExpectedException(typeof(TestException))]
        public async Task Publish_ShouldReThrowExceptionLocatedInCallingAssembly()
        {
            var requestCommand = new AddProductCommand("RoutingKey", 42);
            var exception = new TestException("Some exception message");

            var responseCommand = new ResponseCommandMessage(
                message: JsonConvert.SerializeObject(exception),
                type: exception.GetType().Name,
                correlationId: requestCommand.CorrelationId,
                timestamp: requestCommand.Timestamp
            );

            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendCommandAsync(It.IsAny<RequestCommandMessage>()))
                .ReturnsAsync(responseCommand);

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);

            var target = new CommandPublisher(contextMock.Object);

            await target.Publish<int>(requestCommand);
        }

        [TestMethod, ExpectedException(typeof(Exception))]
        public async Task Publish_ShouldReThrowExceptionWhenNoExceptionHasBeenFound()
        {
            var requestCommand = new AddProductCommand("RoutingKey", 42);
            var exception = new BusConfigurationException("Some exception message");

            var responseCommand = new ResponseCommandMessage(
                message: JsonConvert.SerializeObject(exception),
                type: exception.GetType().Name,
                correlationId: requestCommand.CorrelationId,
                timestamp: requestCommand.Timestamp
            );

            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendCommandAsync(It.IsAny<RequestCommandMessage>()))
                .ReturnsAsync(responseCommand);

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);

            var target = new CommandPublisher(contextMock.Object);

            await target.Publish<int>(requestCommand);
        }

        [TestMethod, ExpectedException(typeof(InvalidCastException))]
        public async Task Publish_ShouldThrowInvalidCastExceptionWhenUnableToCastTheOccuredException()
        {
            var requestCommand = new AddProductCommand("RoutingKey", 42);
            var exception = new InvalidException();

            var responseCommand = new ResponseCommandMessage(
                message: JsonConvert.SerializeObject(exception),
                type: exception.GetType().Name,
                correlationId: requestCommand.CorrelationId,
                timestamp: requestCommand.Timestamp
            );

            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendCommandAsync(It.IsAny<RequestCommandMessage>()))
                .ReturnsAsync(responseCommand);

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);

            var target = new CommandPublisher(contextMock.Object);

            await target.Publish<int>(requestCommand);
        }

        [TestMethod, ExpectedException(typeof(ObjectDisposedException))]
        public async Task Publish_ShouldThrowExceptionWhenDisposed()
        {
            var commandSenderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            commandSenderMock.Setup(s => s.Dispose());

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(commandSenderMock.Object);

            var command = new AddOrderCommand("RoutinKey", new Order());
            var target = new CommandPublisher(contextMock.Object);

            target.Dispose();

            await target.Publish<long>(command);
        }

        [TestMethod, ExpectedException(typeof(BusConfigurationException))]
        public async Task Publish_ShouldThrowExceptionFromExceptionTypesDictionary()
        {
            var exceptionType = typeof(BusConfigurationException);
            var exceptions = new Dictionary<string, Type> { { exceptionType.Name, exceptionType } };
            CommandPublisher.ExceptionTypes = exceptions;

            var requestCommand = new AddProductCommand("RoutingKey", 42);
            var exception = new BusConfigurationException("Exception message");

            var responseCommand = new ResponseCommandMessage(
                message: JsonConvert.SerializeObject(exception),
                type: exception.GetType().Name,
                correlationId: requestCommand.CorrelationId,
                timestamp: requestCommand.Timestamp
            );

            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.SendCommandAsync(It.IsAny<RequestCommandMessage>()))
                .ReturnsAsync(responseCommand);

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);

            var target = new CommandPublisher(contextMock.Object);

            await target.Publish<int>(requestCommand);
        }

        [TestMethod]
        public void Dispose_ShouldCallDisposeOnResources()
        {
            var senderMock = new Mock<ICommandSender>(MockBehavior.Strict);
            senderMock.Setup(s => s.Dispose());

            var contextMock = new Mock<IBusContext<IConnection>>(MockBehavior.Strict);
            contextMock.Setup(ctx => ctx.CreateCommandSender()).Returns(senderMock.Object);
            
            var target = new CommandPublisher(contextMock.Object);
            target.Dispose();
            target.Dispose(); // Don't call dispose the second time

            senderMock.Verify(s => s.Dispose(), Times.Once);
        }
    }
}