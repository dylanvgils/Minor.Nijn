using Minor.Nijn.WebScale.Attributes;

namespace Minor.Nijn.WebScale.Test.TestClasses
{
    [EventListener(TestClassesConstants.AuditEventListenerQueueName)]
    public class AuditEventListener
    {
        public static bool HandleEventsHasBeenCalled;
        public static EventMessage HandleEventsHasBeenCalledWith;

        [Topic("#")]
        public void HandleEvents(EventMessage message)
        {
            HandleEventsHasBeenCalled = true;
            HandleEventsHasBeenCalledWith = message;
        }
    }
}