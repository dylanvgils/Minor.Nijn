using Minor.Nijn.TestBus.CommandBus;
using Minor.Nijn.TestBus.EventBus;

namespace Minor.Nijn.TestBus
{
    internal interface IBusContextExtension : IBusContext<object>
    {
        IEventBus EventBus { get; }
        ICommandBus CommandBus { get; }

        ITestCommandSender CreateTestCommandSender();
    }
}