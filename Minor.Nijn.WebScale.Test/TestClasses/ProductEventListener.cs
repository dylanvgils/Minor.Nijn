using Minor.Nijn.WebScale.Attributes;
using Minor.Nijn.WebScale.Test.TestClasses.Events;
using Minor.Nijn.WebScale.Test.TestClasses.Injectable;

namespace Minor.Nijn.WebScale.Test.TestClasses
{
    [EventListener(TestClassesConstants.ProductEventListenerQueueName)]
    public class ProductEventListener
    {
        public ProductEventListener(IFoo injectable)
        {
            injectable.HasBeenInstantiated = true;
        }

        [Topic(TestClassesConstants.ProductEventHandlerTopic)]
        public void HandleProductAddedEvent(ProductAddedEvent evt)
        {
        }
    }
}