using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;

namespace Conciliacao.Api.Tests.Fixtures
{
    /// <summary>
    /// Implementação em memória de <see cref="IExternalEntryRepository"/> para testes de integração da API.
    /// </summary>
    public class FakeExternalEntryRepository : IExternalEntryRepository
    {
        private readonly List<ExternalEntry> _items = new();

        public Task AddAsync(ExternalEntry externalEntry)
        {
            _items.Add(externalEntry);
            return Task.CompletedTask;
        }

        public Task AddRangeAsync(IEnumerable<ExternalEntry> entries)
        {
            _items.AddRange(entries);
            return Task.CompletedTask;
        }

        public Task<ExternalEntry?> GetByReferenceAsync(string reference)
        {
            var found = _items.FirstOrDefault(e => e.Reference == reference);
            return Task.FromResult<ExternalEntry?>(found);
        }

        public Task<List<ExternalEntry>> GetAllAsync()
        {
            return Task.FromResult(new List<ExternalEntry>(_items));
        }
    }
}
