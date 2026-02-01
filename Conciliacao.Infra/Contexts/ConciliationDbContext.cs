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
        public DbSet<ProcessedRequest> ProcessedRequests { get; set; } = null!;

        public async Task CommitAsync()
        {
            await SaveChangesAsync();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Conciliation>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.ExternalReference)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasIndex(x => x.ExternalReference)
                      .IsUnique();
            });

            modelBuilder.Entity<ProcessedRequest>(entity =>
            {
                entity.Property(e => e.IdempotencyKey)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasIndex(e => e.IdempotencyKey)
                    .IsUnique();
            });
        }
    }
}