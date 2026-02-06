using Conciliacao.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infra.Contexts
{
    public class ConciliationDbContext : DbContext
    {
        public ConciliationDbContext(DbContextOptions<ConciliationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ExternalEntry> ExternalEntries { get; set; }
        public DbSet<ProcessedRequest> ProcessedRequests { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);
        }
    }
}