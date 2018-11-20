﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.TestBus;
using Moq;
using System.Collections.Generic;
using Minor.Nijn.TestBus.EventBus;
using System;

namespace Minor.Nijn.TestBus.EventBus.Test
{
    [TestClass]
    public class TestMessageReceiverTest
    {
        private string queueName;
        private IEnumerable<string> topicExpressions;
        private Mock<ITestBusContext> contextMock;
        private TestMessageReceiver target;

        [TestInitialize]
        public void BeforeEach()
        {
            queueName = "TestQueue";
            topicExpressions = new List<string> { "a.b.c" };

            contextMock = new Mock<ITestBusContext>(MockBehavior.Strict);
            target = new TestMessageReceiver(contextMock.Object, queueName, topicExpressions);
        }

        [TestMethod]
        public void DeclareQueue_ShouldDeclareAQueueOnTheEventBuzz()
        {
            contextMock.Setup(context => context.EventBus.DeclareQueue(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(new EventBusQueue(queueName, topicExpressions));

            target.DeclareQueue();

            contextMock.Verify(context => context.EventBus.DeclareQueue(queueName, topicExpressions));
        }

        [TestMethod]
        public void StartReceivingMessages_ShouldStartListeningForMessages()
        {
            var callbackMock = new Mock<EventMessageReceivedCallback>(MockBehavior.Strict);
            callbackMock.Setup(callback => callback(It.IsAny<EventMessage>()));

            EventMessage message = new EventMessage("a.b.c", "TestMessage");
            var queue = new EventBusQueue(queueName, topicExpressions);
            contextMock.Setup(context => context.EventBus.DeclareQueue(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .Returns(queue);

            target.DeclareQueue();
            target.StartReceivingMessages(callbackMock.Object);
            queue.Enqueue(message);

            callbackMock.Verify(callback => callback(message));
        }

        [TestMethod]
        public void StartReceivingMessages_ShouldThrowBusConfigurationExceptionWhenQueueIsNotDeclared()
        {
            var callbackMock = new Mock<EventMessageReceivedCallback>(MockBehavior.Strict);
            EventMessage message = new EventMessage("a.b.c", "TestMessage");

            Action action = () =>
            {
                target.StartReceivingMessages(callbackMock.Object);
            };

            var ex = Assert.ThrowsException<BusConfigurationException>(action);
            Assert.AreEqual($"Queue with name: {queueName} is not declared", ex.Message);
        }

        [TestMethod]
        public void Dispose_ShouldNotThrowException()
        {
            target.Dispose();
        }
    }
}
