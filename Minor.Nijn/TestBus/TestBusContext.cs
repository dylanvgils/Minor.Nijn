﻿using System;
using System.Collections.Generic;
using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContext : ITestBusContext
    {
        private readonly string _commandQueueName;
        string ITestBusContext.CommandQueueName => _commandQueueName;

        private readonly IEventBus _eventBus;
        IEventBus ITestBusContext.EventBus => _eventBus;

        private readonly ICommandBus _commandBus;
        ICommandBus ITestBusContext.CommandBus => _commandBus;

        public object Connection => throw new NotImplementedException();
        public string ExchangeName => throw new NotImplementedException();

        private TestBusContext() { }

        internal TestBusContext(IEventBus testBus, ICommandBus commandBus, string commandQueueName)
        {
            _eventBus = testBus;
            _commandBus = commandBus;
            _commandQueueName = commandQueueName;
        }

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            var receiver = new TestMessageReceiver(this, queueName, topicExpressions);
            receiver.DeclareQueue();
            return receiver;
        }

        public IMessageSender CreateMessageSender()
        {
            var sender = new TestMessageSender(this);
            return sender;
        }

        public ICommandReceiver CreateCommandReceiver()
        {
            return new TestCommandReceiver(this, _commandQueueName);
        }
        
        public ICommandSender CreateCommandSender()
        {
            return CreateMockCommandSender();
        }
        
        public IMockCommandSender CreateMockCommandSender()
        {
            return new TestCommandSender(this);
        }

        public void SendMockCommand(CommandMessage request)
        {
            request.ReplyTo = _commandQueueName;
            _commandBus.DispatchMessage(request);
        }

        public void Dispose() { }
    }
}
