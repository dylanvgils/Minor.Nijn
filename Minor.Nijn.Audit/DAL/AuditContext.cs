using Microsoft.EntityFrameworkCore;
using Minor.Nijn.Audit.Entities;

namespace Minor.Nijn.Audit.DAL
{
    internal class AuditContext : DbContext
    {
        public DbSet<AuditMessage> Messages { get; set; }

        public AuditContext(DbContextOptions options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AuditMessage>(m =>
            {
                m.Property(p => p.RoutingKey)
                    .HasMaxLength(255)
                    .IsRequired();

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