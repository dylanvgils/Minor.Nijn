using System.Collections.Generic;

namespace Minor.Nijn.TestBus.EventBus
{
    public sealed class TestMessageReceiver : IMessageReceiver
    {
        public string QueueName { get; }
        public IEnumerable<string> TopicExpressions { get; }
        private readonly ITestBusContext _context;

        private bool _queueDeclared;
        private EventBusQueue _queue;

        internal TestMessageReceiver(ITestBusContext context, string queueName, IEnumerable<string> topicExpressions)
        {
            _context = context;
            QueueName = queueName;
            TopicExpressions = topicExpressions;
        }

        public void DeclareQueue()
        {
           _queue =  _context.EventBus.DeclareQueue(QueueName, TopicExpressions);
           _queueDeclared = true;
        }

        public void StartReceivingMessages(EventMessageReceivedCallback callback)
        {
            if (!_queueDeclared)
            {
                throw new BusConfigurationException($"Queue with name: {QueueName} is not declared");
            }

            _queue.Subscribe((sender, args) => callback(args.Message));
        }

        public void Dispose() { }
    }
}
