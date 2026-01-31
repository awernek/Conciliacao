using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Repositories
{
    public interface ITransactionRepository
    {
        Task AddAsync(Transaction transaction);
        Task<Transaction?> GetByReferenceAsync(string reference);
        Task AddRangeAsync(IEnumerable<Transaction> transactions);
        Task<List<Transaction>> GetAllAsync();
    }
}