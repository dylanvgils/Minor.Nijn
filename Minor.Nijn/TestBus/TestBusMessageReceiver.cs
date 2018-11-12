using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusMessageReceiver : IMessageReceiver
    {
        public string QueueName { get; private set; }
        public IEnumerable<string> TopicExpressions { get; private set; }

        private readonly TestBusContext _context;

        public TestBusMessageReceiver(TestBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
        }

        public void DeclareQueue()
        {
            _context.DeclareQueue(QueueName);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void StartReceivingMessages(EventMessageReceivedCallback Callback)
        {
            throw new NotImplementedException();
        }
    }
}
