namespace Minor.Nijn.TestBus.CommandBus
{
    public class CommandBusQueue : TestBusQueue<TestBusCommand>
    {               
        public CommandBusQueue(string queueName) : base(queueName) { }
             
        public override void Enqueue(TestBusCommand message)
        {
            if (QueueName == message.RoutingKey)
            {
                base.Enqueue(message);   
            }
        }
    }
}