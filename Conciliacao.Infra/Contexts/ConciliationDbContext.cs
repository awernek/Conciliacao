using Conciliacao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infrastructure.Persistence.Contexts
{
    public class ConciliationDbContext : DbContext
    {
        public ConciliationDbContext(DbContextOptions<ConciliationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ExternalEntry> ExternalEntries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações via Fluent API
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConciliationDbContext).Assembly);
        }
    }
}