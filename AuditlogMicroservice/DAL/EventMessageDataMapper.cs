using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuditlogMicroservice.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditlogMicroservice.DAL
{
    public class EventMessageDataMapper : IDataMapper<EventMessage, long>
    {
        private DbContextOptions<EventMessageContext> options;

        public EventMessageDataMapper(DbContextOptions<EventMessageContext> options)
        {
            this.options = options;
        }

        public void Insert(EventMessage item)
        {
            using (var context = new EventMessageContext(options))
            {
                context.Messages.Add(item);
                context.SaveChanges();
            }
        }
    }
}
