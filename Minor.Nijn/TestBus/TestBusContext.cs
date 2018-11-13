using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContext : IBusContextExtension
    {
        private readonly ITestBuzz _testBuzz;
        ITestBuzz IBusContextExtension.TestBuzz => _testBuzz;

        public object Connection => throw new NotImplementedException();
        public string ExchangeName => throw new NotImplementedException();

        private TestBusContext() { }

        internal TestBusContext(ITestBuzz testBus)
        {
            _testBuzz = testBus;
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

        public ICommandSender CreateCommandSender()
        {
            return new TestBusCommandSender(this);
        }

        public ICommandReceiver CreateCommandReceiver()
        {
            return new TestBusCommandReceiver(this);
        }

        public void Dispose() { }
    }
}
