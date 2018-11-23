namespace Minor.Nijn.TestBus.CommandBus
{
    public class CommandBusQueue : TestBusQueue<CommandMessage>
    {               
        public CommandBusQueue(string queueName) : base(queueName) { }
             
        public override void Enqueue(CommandMessage message)
        {
            if (QueueName == message.RoutingKey)
            {
                base.Enqueue(message);   
            }
        }
    }
}