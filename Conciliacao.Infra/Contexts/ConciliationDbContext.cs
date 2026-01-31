using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infrastructure.Persistence.Contexts
{
    public class ConciliationDbContext
        : DbContext,
        IUnitOfWork
    {
        public ConciliationDbContext(DbContextOptions<ConciliationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<ExternalEntry> ExternalEntries { get; set; }

        public async Task CommitAsync()
        {
            await SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações via Fluent API
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConciliationDbContext).Assembly);
        }
    }
}