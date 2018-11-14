using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditlogMicroservice.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditlogMicroservice.DAL
{
    public class EventDataMapper : IDataMapper<Event, long>
    {
        private DbContextOptions<EventContext> options;

        public EventDataMapper(DbContextOptions<EventContext> options)
        {
            this.options = options;
        }

        public void Insert(Event item)
        {
            using (var context = new EventContext(options))
            {
                context.Messages.Add(item);
                context.SaveChanges();
            }
        }
    }
}
