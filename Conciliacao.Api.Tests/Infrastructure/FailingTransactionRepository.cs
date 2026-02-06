using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;

namespace Conciliacao.Api.Tests.Infrastructure
{
    /// <summary>
    /// Repositório de transações que sempre falha ao tentar salvar, usado para testes.
    /// </summary>
    public class FailingTransactionRepository : ITransactionRepository
    {
        /// <summary>
        /// Simula uma falha ao tentar adicionar uma transação.
        /// </summary> <param name="transaction">A transação a ser adicionada (ignorada).</param>
        public Task AddAsync(Transaction transaction)
        {
            throw new InvalidOperationException("Erro simulado ao salvar Transaction");
        }

        /// <summary> Simula uma falha ao tentar adicionar várias transações. </summary>
        /// <param name="transactions">As transações a serem adicionadas (ignoradas).</param>
        public Task AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            throw new InvalidOperationException("Erro simulado ao salvar Transactions em lote");
        }

        /// <summary>Simula uma falha ao tentar obter uma transação por referência.</summary>
        /// <param name="reference">A referência da transação (ignorada).</param>
        public Task<Transaction?> GetByReferenceAsync(string reference)
            => Task.FromResult<Transaction?>(null);

        /// <summary>Simula uma falha ao tentar obter todas as transações.</summary>
        public Task<List<Transaction>> GetAllAsync()
            => Task.FromResult(new List<Transaction>());
    }
}