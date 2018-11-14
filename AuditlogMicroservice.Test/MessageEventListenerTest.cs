using AuditlogMicroservice.DAL;
using AuditlogMicroservice.Entities;
using AuditlogMicroservice.EventListeners;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace AuditlogMicroservice.Test
{
    [TestClass]
    public class MessageEventListenerTest
    {
        [TestMethod]
        public void HandleEvent_ShouldCallInsertWithCorrectData()
        {
            var mock = new Mock<IDataMapper<Event, long>>(MockBehavior.Strict);
            mock.Setup(m => m.Insert(It.IsAny<Event>()));
            var target = new MessageEventListener(mock.Object);
            var message = new Minor.Nijn.EventMessage("a.b.c", "TestMessage", timestamp: 1234);

            target.HandleEvent(message);

            var resultMessage = new Event { RoutingKey = "a.b.c", Message = "TestMessage", Timestamp = 1234 };
            mock.Verify(m => m.Insert(It.Is<Event>(e => 
                e.RoutingKey == message.RoutingKey && 
                e.Message == message.Message && 
                e.Timestamp == message.Timestamp
                )), Times.Once); 
        }
    }
}
