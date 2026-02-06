using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infra.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ConciliationDbContext _context;

        public TransactionRepository(ConciliationDbContext context)
        {
            _context = context;
        }

        public Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
            return Task.CompletedTask;
        }

        public async Task AddRangeAsync(IEnumerable<Transaction> transactions)
        {
            await _context.Transactions.AddRangeAsync(transactions);
        }

        public async Task<Transaction?> GetByReferenceAsync(string reference)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.Reference == reference);
        }

        public async Task<List<Transaction>> GetAllAsync()
        {
            return await _context.Transactions.ToListAsync();
        }
    }
}