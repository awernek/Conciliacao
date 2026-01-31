using Conciliacao.Domain.Entities;

namespace Conciliacao.Application.Requests
{
    public class ConciliationRequest
    {
        public List<ConciliationItem> Items { get; set; } = new();

        public List<Transaction> ToTransactions()
        {
            return Items.Select(item =>
                new Transaction("", item.Reference, item.Amount, default)
            ).ToList();
        }
    }
}