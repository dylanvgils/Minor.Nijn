using System;
using System.Collections.Generic;
using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContext : ITestBusContext
    {
        public IConnection Connection => throw new NotImplementedException();
        public string ExchangeName => throw new NotImplementedException();
        public IEventBus EventBus { get; }
        public ICommandBus CommandBus { get; }

        internal TestBusContext(IEventBus testBus, ICommandBus commandBus)
        {
            EventBus = testBus;
            CommandBus = commandBus;
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            return new TestMessageReceiver(this, queueName, topicExpressions);
        }

        public IMessageSender CreateMessageSender()
        {
            var sender = new TestMessageSender(this);
            return sender;
        }

        public ICommandReceiver CreateCommandReceiver(string queueName)
        {
            return new TestCommandReceiver(this, queueName);
        }
        
        public ICommandSender CreateCommandSender()
        {
            return new TestCommandSender(this);
        }

        public void Dispose() { }
    }
}
