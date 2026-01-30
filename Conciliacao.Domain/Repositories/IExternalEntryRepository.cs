using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Repositories
{
    public interface IExternalEntryRepository
    {
        Task AddAsync(ExternalEntry externalEntry);
        Task AddRangeAsync(IEnumerable<ExternalEntry> entries);
        Task<ExternalEntry?> GetByReferenceAsync(string reference);
        Task<List<ExternalEntry>> GetAllAsync();
    }
}