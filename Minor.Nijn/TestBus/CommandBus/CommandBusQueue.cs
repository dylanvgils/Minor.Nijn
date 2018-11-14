namespace Minor.Nijn.TestBus.CommandBus
{
    internal class CommandBusQueue : TestBusQueue<CommandMessage>
    {       
        internal CommandBusQueue() { }
        
        public CommandBusQueue(string queueName) : base(queueName) { }
             
        public override void Enqueue(CommandMessage message)
        {
            if (QueueName == message.ReplyTo)
            {
                base.Enqueue(message);   
            }
        }
    }
}