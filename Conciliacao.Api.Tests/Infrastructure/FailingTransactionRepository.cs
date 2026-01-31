using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;

namespace Conciliacao.Api.Tests.Infrastructure
{
    public class FailingTransactionRepository : ITransactionRepository
    {
        public Task AddAsync(Transaction transaction)
        {
            throw new InvalidOperationException("Erro simulado ao salvar Transaction");
        }

        public Task AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            throw new InvalidOperationException("Erro simulado ao salvar Transactions em lote");
        }

        public Task<Transaction?> GetByReferenceAsync(string reference)
            => Task.FromResult<Transaction?>(null);

        public Task<List<Transaction>> GetAllAsync()
            => Task.FromResult(new List<Transaction>());
    }
}