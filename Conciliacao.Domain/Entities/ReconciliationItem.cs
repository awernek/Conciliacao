using Conciliacao.Domain.Enums;

namespace Conciliacao.Domain.Entities
{
    public class ReconciliationItem
    {
        public Transaction? Transaction { get; }
        public ExternalEntry? ExternalEntry { get; }
        public ReconciliationResult Result { get; }

        public ReconciliationItem(
            Transaction? transaction,
            ExternalEntry? externalEntry,
            ReconciliationResult result)
        {
            Transaction = transaction;
            ExternalEntry = externalEntry;
            Result = result;
        }
    }
}