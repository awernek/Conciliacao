using Conciliacao.Domain.Entities;

namespace Conciliacao.Application.Models
{
    public class ReconciliationBatchResult
    {
        public List<(Transaction Transaction, ExternalEntry ExternalEntry)> Matched { get; } = new();
        public List<(Transaction Transaction, ExternalEntry ExternalEntry)> Divergent { get; } = new();
        public List<Transaction> Missing { get; } = new();
        public List<ExternalEntry> Extra { get; } = new();
    }
}