using System;
using System.Collections.Generic;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusMessageReceiver : IMessageReceiver
    {
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }
        private TestBuzzQueue _queue;

        private readonly IBusContextExtension _context;

        internal TestBusMessageReceiver(IBusContextExtension context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
        }

        public void DeclareQueue()
        {
           _queue =  _context.TestBuzz.DeclareQueue(QueueName, TopicExpressions);
        }

        public void StartReceivingMessages(EventMessageReceivedCallback Callback)
        {
            _queue.MessageAdded += (object sender, MessageAddedEventArgs args) =>
            {
                Callback(args.Message);
            };
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
