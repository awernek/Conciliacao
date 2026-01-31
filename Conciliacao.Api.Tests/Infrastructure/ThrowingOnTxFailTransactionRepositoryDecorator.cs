using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;

namespace Conciliacao.Api.Tests.Infrastructure
{
    /// <summary>
    /// Decorator que lança exceção quando alguma transação tem Reference "TX_FAIL",
    /// para testes de rollback (Should_rollback_transaction_when_error_occurs).
    /// </summary>
    public class ThrowingOnTxFailTransactionRepositoryDecorator : ITransactionRepository
    {
        private const string FailReference = "TX_FAIL";
        private readonly ITransactionRepository _inner;

        public ThrowingOnTxFailTransactionRepositoryDecorator(ITransactionRepository inner)
        {
            _inner = inner;
        }

        public Task AddAsync(Transaction transaction)
        {
            if (transaction.Reference == FailReference)
                throw new InvalidOperationException("Erro simulado para teste de rollback.");
            return _inner.AddAsync(transaction);
        }

        public Task AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            var list = transactions.ToList();
            if (list.Any(t => t.Reference == FailReference))
                throw new InvalidOperationException("Erro simulado para teste de rollback.");
            return _inner.AddRangeAsync(list);
        }

        public Task<Transaction?> GetByReferenceAsync(string reference)
            => _inner.GetByReferenceAsync(reference);

        public Task<List<Transaction>> GetAllAsync()
            => _inner.GetAllAsync();
    }
}
