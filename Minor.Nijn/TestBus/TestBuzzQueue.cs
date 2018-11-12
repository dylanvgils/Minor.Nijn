using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.TestBus
{
    public class TestBuzzQueue
    {
        public event EventHandler<MessageAddedEventArgs> MessageAdded;
        private readonly List<string> _topicExpressions;

        public TestBuzzQueue(IEnumerable<string> topicExpressions)
        {
            _topicExpressions = topicExpressions.ToList();
        }

        public void Enqueue(EventMessage message)
        {
            if (_topicExpressions.Contains(message.RoutingKey))
            {
                OnMessageAdded(new MessageAddedEventArgs(message));
            }
        }

        protected virtual void OnMessageAdded(MessageAddedEventArgs args)
        {
            MessageAdded?.Invoke(this, args);
        }
    }
}
