using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.TestBus
{
    internal abstract class TestBusQueue<T>
    {
        private event EventHandler<MessageAddedEventArgs<T>> MessageAdded;
        private readonly Queue<T> _messageQueue;

        public string QueueName { get; }
        public int MessageQueueLength => _messageQueue.Count;
        public int SubscriberCount => MessageAdded?.GetInvocationList().Length ?? 0;

        internal TestBusQueue() {}

        protected TestBusQueue(string name)
        {
            QueueName = name;
            _messageQueue = new Queue<T>();
        }

        public virtual void Enqueue(T message)
        {
            OnMessageAdded(new MessageAddedEventArgs<T>(message));
        }

        public virtual void Subscribe(EventHandler<MessageAddedEventArgs<T>> handler)
        {
            MessageAdded += handler;

            foreach (var message in _messageQueue.ToList())
            {
                Enqueue(_messageQueue.Dequeue());
            }
        }

        public virtual void Unsubscribe(EventHandler<MessageAddedEventArgs<T>> handler)
        {
            MessageAdded -= handler;
        }

        protected virtual void OnMessageAdded(MessageAddedEventArgs<T> args)
        {
            if (SubscriberCount > 0)
            {
                MessageAdded?.Invoke(this, args);
                return;
            }

            _messageQueue.Enqueue(args.Message);
        }
    }
}
