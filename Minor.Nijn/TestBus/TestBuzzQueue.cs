using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.TestBus
{
    internal class TestBuzzQueue
    {
        public event EventHandler<MessageAddedEventArgs> MessageAdded;
        private readonly IEnumerable<string> _topicExpressions;

        public TestBuzzQueue(IEnumerable<string> topicExpressions)
        {
            _topicExpressions = topicExpressions;
        }

        public void Enqueue(EventMessage message)
        {
            if (TopicMatcher.IsMatch(_topicExpressions, message.RoutingKey))
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
