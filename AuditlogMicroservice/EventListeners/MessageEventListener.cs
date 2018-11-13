using Minor.Nijn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditlogMicroservice.EventListeners
{
    public class MessageEventListener
    {

        public void HandleEvent(EventMessage message)
        {
            Console.WriteLine(message.Message);
        }
    }
}
