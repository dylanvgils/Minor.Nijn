using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.TestBus
{
    public class TestBuzzQueue
    {
        public event EventHandler<MessageAddedEventArgs> MessageAdded;
        public List<string> TopicExpressions { get; private set; }

        public TestBuzzQueue(IEnumerable<string> topicExpressions)
        {
            TopicExpressions = topicExpressions.ToList();
        }

        public void Enqueue(EventMessage message)
        {
            if (TopicExpressions.Contains(message.RoutingKey))
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
