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
        public void ContextCanBeBuild()
        {
            var connectionBuilder = new RabbitMQContextBuilder()
                    .WithExchange("MVM.EventExchange")
                    .WithAddress("localhost", 5672)
                    .WithCredentials(userName: "guest", password: "guest");

            var context = connectionBuilder.CreateContext();

            Assert.AreEqual("MVM.EventExchange", context.ExchangeName);
        }
    }
}
