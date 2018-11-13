using System;
using System.Collections.Generic;
using System.Text;

namespace Minor.Nijn.TestBus
{
    internal interface ITestBuzz
    {
        void DispatchMessage(EventMessage message);
        TestBuzzQueue DeclareQueue(string queueName, IEnumerable<string> topicExpressions);
    }
}
