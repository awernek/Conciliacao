using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;

namespace Conciliacao.Api.Tests.Fixtures
{
    /// <summary>
    /// Implementação em memória de <see cref="ITransactionRepository"/> para testes de integração da API.
    /// </summary>
    public class FakeTransactionRepository : ITransactionRepository
    {
        private readonly List<Transaction> _items = new();

        public Task AddAsync(Transaction transaction)
        {
            _items.Add(transaction);
            return Task.CompletedTask;
        }

        public Task<Transaction?> GetByReferenceAsync(string reference)
        {
            var found = _items.FirstOrDefault(t => t.Reference == reference);
            return Task.FromResult<Transaction?>(found);
        }

        public Task<List<Transaction>> GetAllAsync()
        {
            return Task.FromResult(new List<Transaction>(_items));
        }
    }
}
