using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus
{
    internal sealed class TestBuzz : ITestBuzz
    {
        private IDictionary<string, TestBuzzQueue> _queues;
        public int QueueLenght => _queues.Count;

        public TestBuzz()
        {
            _queues = new Dictionary<string, TestBuzzQueue>();
        }

        public void DispatchMessage(EventMessage message)
        {
            foreach (var queue in _queues)
            {
                queue.Value.Enqueue(message);
            }
        }

        public TestBuzzQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions)
        {
            if (!_queues.ContainsKey(queueName))
            {
                var queue = new TestBuzzQueue(topicExpressions);
                _queues.Add(queueName, queue);
                return queue;
            }

            return _queues[queueName];
        }
    }
}
