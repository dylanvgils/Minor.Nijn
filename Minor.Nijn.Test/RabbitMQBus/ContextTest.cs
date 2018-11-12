using Microsoft.VisualStudio.TestTools.UnitTesting;
using Minor.Nijn.RabbitMQBus;
using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.Test.RabbitMQBus
{
    [TestClass]
    public class ContextTest
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
    }
}
