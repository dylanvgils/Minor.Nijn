using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Events;

namespace Minor.Nijn.WebScale.Test.TestClasses
{
    [EventListener(TestClassesConstants.ProductEventListenerQueueName)]
    public class ProductEventListener
    {
        [Topic(TestClassesConstants.ProductEventHandlerTopic)]
        public void HandleProductAddedEvent(ProductAddedEvent evt)
        {
        }
    }
}