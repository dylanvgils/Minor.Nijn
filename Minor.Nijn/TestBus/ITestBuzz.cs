using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus
{
    public interface ITestBuzz
    {
        void DispatchMessage(EventMessage message);
        void DeclareQueue(string queueName, IEnumerable<string> topicExpressions);
    }
}
