using System;

namespace Minor.Nijn.TestBus.CommandBus
{
    public sealed class TestCommandReceiver : ICommandReceiver
    {
        public string QueueName { get; }
        private readonly ITestBusContext _context;

        private bool _queueDeclared;
        private CommandBusQueue _queue;
       
        internal TestCommandReceiver(ITestBusContext context, string queueName)
        {
            QueueName = queueName;
            _context = context;
        }
        
        public void DeclareCommandQueue()
        {
            _queue = _context.CommandBus.DeclareCommandQueue(QueueName);
            _queueDeclared = true;
        }

        public void StartReceivingCommands(CommandReceivedCallback callback)
        {
            if (!_queueDeclared)
            {
                throw new BusConfigurationException($"Queue with name: {QueueName} is not declared");
            }

            _queue.Subscribe((sender, args) =>
            {
                if (args.Message.ReplyTo != null && _context.CommandBus.Queues.ContainsKey(args.Message.ReplyTo))
                {
                    var response = callback(args.Message.Command as RequestCommandMessage);
                    response.RoutingKey = args.Message.ReplyTo;

                    var responseCommand = new TestBusCommand(null, response);
                    _context.CommandBus.Queues[args.Message.ReplyTo].Enqueue(responseCommand);

                    return;
                }

                callback(args.Message.Command as RequestCommandMessage);
            });
        }
        
        public void Dispose() { }
    }
}