using System.Collections.Generic;
using RabbitMQ.Client;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBusContext : IBusContext<object>
    {

        private IDictionary<string, Queue<EventMessage>> _queues;
        public int QueueLenght => _queues.Count;

        public TestBusContext()
        {
            _queues = new Dictionary<string, Queue<EventMessage>>();
        }

        public object Connection => throw new System.NotImplementedException();

        public string ExchangeName => throw new System.NotImplementedException();

        public IMessageReceiver CreateMessageReceiver(string queueName, IEnumerable<string> topicExpressions)
        {
            var receiver = new TestBusMessageReceiver(this, queueName, topicExpressions);
            receiver.DeclareQueue();
            return receiver;
        }

        public IMessageSender CreateMessageSender()
        {
            throw new System.NotImplementedException();
        }

        public void DeclareQueue(string queueName)
        {
            if (!_queues.ContainsKey(queueName))
            {
                _queues.Add(queueName, new Queue<EventMessage>());
            }
        }

        public void Dispose()
        {
            throw new System.NotImplementedException();
        }
    }
}
