using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infrastructure.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Infrastructure.Persistence.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ConciliationDbContext _context;

        public TransactionRepository(ConciliationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Transaction transaction)
        {
            _context.Transactions.Add(transaction);
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