using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;

namespace Minor.Nijn.TestBus
{
    internal interface ITestBusContext : IBusContext<object>
    {
        IEventBus EventBus { get; }
        ICommandBus CommandBus { get; }

        IMockCommandSender CreateMockCommandSender();
        void SendMockCommand(CommandMessage request);
    }
}