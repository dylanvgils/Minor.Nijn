using AuditlogMicroservice.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuditlogMicroservice.DAL
{
    public class EventContext : DbContext
    {
        public EventContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Event> Messages { get; set; }
    }
}
