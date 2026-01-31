using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Repositories
{
    public interface IProcessedRequestRepository
    {
        Task<ProcessedRequest?> GetByKeyAsync(string idempotencyKey);

        Task AddAsync(ProcessedRequest request);
    }
}
