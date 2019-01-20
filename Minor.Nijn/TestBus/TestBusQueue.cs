using System;
using System.Collections.Generic;
using System.Linq;

namespace Minor.Nijn.TestBus
{
    public abstract class TestBusQueue<T>
    {
        private event EventHandler<MessageAddedEventArgs<T>> MessageAdded;
        private readonly Queue<T> _messageQueue;

        public string QueueName { get; }
        public int MessageQueueLength => _messageQueue.Count;
        public int SubscriberCount => MessageAdded?.GetInvocationList().Length ?? 0;
        public int CalledTimes { get; private set; }

        public T this[int n] => FindItemInQueue(n);

        protected TestBusQueue(string name)
        {
            QueueName = name;
            _messageQueue = new Queue<T>();
        }

        public virtual void Enqueue(T message)
        {
            CalledTimes++;
            OnMessageAdded(new MessageAddedEventArgs<T>(message));
        }

        internal virtual void Subscribe(EventHandler<MessageAddedEventArgs<T>> handler)
        {
            MessageAdded += handler;
            _messageQueue.ToList().ForEach(m => Enqueue(_messageQueue.Dequeue()));
        }

        internal virtual void Unsubscribe(EventHandler<MessageAddedEventArgs<T>> handler)
        {
            MessageAdded -= handler;
        }

        internal virtual void OnMessageAdded(MessageAddedEventArgs<T> args)
        {
            if (SubscriberCount > 0)
            {
                MessageAdded?.Invoke(this, args);
                return;
            }

            _messageQueue.Enqueue(args.Message);
        }

        private T FindItemInQueue(int index)
        {
            return _messageQueue.AsEnumerable().ElementAtOrDefault(index);
        }
    }
}
