using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus
{
    public sealed class TestBuzz : ITestBuzz
    {
        private IDictionary<string, Queue<EventMessage>> _queues;
        public int QueueLenght => _queues.Count;
        public event EventHandler<MessageAddedEventArgs> MessageAdded;

        public TestBuzz()
        {
            _queues = new Dictionary<string, Queue<EventMessage>>();
        }

        //TODO: ffgoeddoenofzo
        public void DispatchMessage(EventMessage message)
        {
            MessageAdded?.Invoke(this, new MessageAddedEventArgs(message));
        }

        public void DeclareQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            if (!_queues.ContainsKey(queueName))
            {
                _queues.Add(queueName, new Queue<EventMessage>());
            }
        }
    }
}
