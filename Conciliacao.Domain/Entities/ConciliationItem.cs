using Conciliacao.Domain.Enums;

namespace Conciliacao.Domain.Entities
{
    public class ConciliationItem
    {
        public Transaction? Transaction { get; }
        public ExternalEntry? ExternalEntry { get; }
        public ConciliationStatus Status { get; }

        public ConciliationItem(
            Transaction? transaction,
            ExternalEntry? externalEntry,
            ConciliationStatus status)
        {
            Transaction = transaction;
            ExternalEntry = externalEntry;
            Status = status;
        }
    }
}
