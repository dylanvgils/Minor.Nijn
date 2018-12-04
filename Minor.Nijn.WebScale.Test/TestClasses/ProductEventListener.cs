using System.Threading;
using System.Threading.Tasks;
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
        public async Task HandleProductAddedEvent(ProductAddedEvent evt)
        {
            var task = Task.Run(() => { Thread.Sleep(1500); });
            await task;
        }
    }
}