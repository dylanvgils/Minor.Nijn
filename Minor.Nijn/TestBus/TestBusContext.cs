using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContext : IBusContextExtension
    {
        public ITestBuzz TestBus { get; private set; }
        public object Connection => throw new System.NotImplementedException();
        public string ExchangeName => throw new System.NotImplementedException();

        public TestBusContext(ITestBuzz testBus)
        {
            TestBus = testBus;
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            var receiver = new TestBusMessageReceiver(this, queueName, topicExpressions);
            receiver.DeclareQueue();
            return receiver;
        }

        public IMessageSender CreateMessageSender()
        {
            var sender = new TestBusMessageSender(this);
            return sender;
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }


    }
}
