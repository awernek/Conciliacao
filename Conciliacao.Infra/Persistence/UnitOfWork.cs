using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Contexts;

namespace Conciliacao.Infra.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ConciliationDbContext _context;

        public UnitOfWork(ConciliationDbContext context)
        {
            _context = context;
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
