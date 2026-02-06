using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infra.Repositories
{
    public class ProcessedRequestRepository : IProcessedRequestRepository
    {
        private readonly ConciliationDbContext _context;

        public ProcessedRequestRepository(ConciliationDbContext context)
        {
            _context = context;
        }

        public async Task<ProcessedRequest?> GetByKeyAsync(string idempotencyKey)
        {
            return await _context.ProcessedRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IdempotencyKey == idempotencyKey);
        }

        public Task AddAsync(ProcessedRequest request)
        {
            _context.ProcessedRequests.Add(request);
            return Task.CompletedTask;
        }
    }
}