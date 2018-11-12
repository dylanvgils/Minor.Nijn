using Minor.Nijn.WebScale;
using System;
using System.Collections.Generic;
using System.Text;

namespace VoorbeeldMicroservice
{
    /*
    [EventListener("MVM.TestService.PolisEventListenerQueue")]
    public class PolisEventListener
    {
        private readonly IDbContextOptions<PolisContext> _context;

        public PolisEventListener(IDbContextOptions<PolisContext> context)
        {
            _context = context;
        }

        [Topic("MVM.Polisbeheer.PolisToegevoegd")]
        public void Handles(PolisToegevoegdEvent evt)
        {
            _context.Polissen.Add(evt.Polis);
            _context.SaveChanges();
        }
    }
    */
}
