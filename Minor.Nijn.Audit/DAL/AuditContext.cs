using Microsoft.EntityFrameworkCore;
using Minor.Nijn.Audit.Entities;

namespace Minor.Nijn.Audit.DAL
{
    public class AuditContext : DbContext
    {
        public DbSet<AuditMessage> AuditMessages { get; set; }

        public AuditContext(DbContextOptions<AuditContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditMessage>(m =>
            {
                m.Property(p => p.RoutingKey)
                    .HasMaxLength(255)
                    .IsRequired();

                m.Property(p => p.CorrelationId)
                    .HasMaxLength(255);

                m.Property(p => p.Type)
                    .HasMaxLength(255)
                    .IsRequired();

                m.Property(p => p.Timestamp)
                    .IsRequired();

                m.Property(p => p.Payload)
                    .HasMaxLength(10000)
                    .IsRequired();
            });
        }
    }
}