using Conciliacao.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Api.Tests.Infrastructure
{
    public class TestConciliationDbContext : ConciliationDbContext
    {
        public int SaveChangesCallCount { get; private set; }
        private readonly SaveChangesCallCounter? _sharedCounter;

        public TestConciliationDbContext(
            DbContextOptions<ConciliationDbContext> options,
            SaveChangesCallCounter? sharedCounter = null)
            : base(options)
        {
            _sharedCounter = sharedCounter;
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            _sharedCounter?.Increment();
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}