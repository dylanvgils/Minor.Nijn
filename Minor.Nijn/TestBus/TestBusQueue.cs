using System;
using System.Collections.Generic;

namespace Minor.Nijn.TestBus
{
    internal abstract class TestBusQueue<T>
    {
        public event EventHandler<MessageAddedEventArgs<T>> MessageAdded;
        
        public string Name { get; }

        public TestBusQueue(string name)
        {
            Name = name;
        }

        public virtual void Enqueue(T message)
        {
            OnMessageAdded(new MessageAddedEventArgs<T>(message));
        }

        protected virtual void OnMessageAdded(MessageAddedEventArgs<T> args)
        {
            MessageAdded?.Invoke(this, args);
        }
    }
}
