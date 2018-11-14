using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    public sealed class TestMessageReceiver : IMessageReceiver
    {
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }
        private readonly IBusContextExtension _context;
        private EventBusQueue _queue;

        private TestMessageReceiver() { }

        internal TestMessageReceiver(IBusContextExtension context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
        }

        public void DeclareQueue()
        {
           _queue =  _context.EventBus.DeclareQueue(QueueName, TopicExpressions);
        }

        public void StartReceivingMessages(EventMessageReceivedCallback callback)
        {
            _queue.Subscribe((sender, args) => callback(args.Message));
        }

        public void Dispose() { }
    }
}
