using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusMessageReceiver : IMessageReceiver
    {
        public string QueueName { get; private set; }
        public IEnumerable<string> TopicExpressions { get; private set; }

        private readonly IBusContextExtension _context;

        public TestBusMessageReceiver(IBusContextExtension context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
        }

        public void DeclareQueue()
        {
            _context.TestBus.DeclareQueue(QueueName, TopicExpressions);
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void StartReceivingMessages(EventMessageReceivedCallback Callback)
        {
            throw new NotImplementedException();
            //_context.MessageAdded += (object sender, MessageAddedEventArgs args) =>
            //{
            //    Callback(args.Message);
            //};
        }
    }
}
