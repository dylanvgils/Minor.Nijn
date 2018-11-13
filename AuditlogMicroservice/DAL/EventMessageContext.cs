using AuditlogMicroservice.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditlogMicroservice.DAL
{
    public class EventMessageContext : DbContext
    {
        public EventMessageContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<EventMessage> Messages { get; set; }
    }
}
