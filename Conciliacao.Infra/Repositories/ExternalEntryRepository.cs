using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infra.Repositories
{
    public class ExternalEntryRepository : IExternalEntryRepository
    {
        private readonly ConciliationDbContext _context;

        public ExternalEntryRepository(ConciliationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ExternalEntry externalEntry)
        {
            _context.ExternalEntries.Add(externalEntry);
        }

        public async Task AddRangeAsync(IEnumerable<ExternalEntry> entries)
        {
            await _context.ExternalEntries.AddRangeAsync(entries);
        }

        public async Task<List<ExternalEntry>> GetAllAsync()
        {
            return await _context.ExternalEntries.ToListAsync();
        }

        public async Task<ExternalEntry?> GetByReferenceAsync(string reference)
        {
            return await _context.ExternalEntries
                .FirstOrDefaultAsync(e => e.Reference == reference);
        }
    }
}