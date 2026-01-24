using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.ValueObjects;

namespace Conciliacao.Domain.Services
{
    public class SimpleReconciliationService
    {
        public bool Match(Transaction transaction, ExternalEntry externalEntry)
        {
            return transaction.Reference == externalEntry.Reference
                && transaction.Amount == externalEntry.Amount
                && transaction.Date == externalEntry.Date;
        }

        public ReconciliationResult Reconcile(
            Transaction? transaction,
            ExternalEntry? externalEntry)
        {
            if (transaction == null
                && externalEntry != null)
            {
                return ReconciliationResult.Extra;
            }

            if (transaction != null
                && externalEntry == null)
            {
                return ReconciliationResult.Missing;
            }

            if (Match(transaction!, externalEntry!))
            {
                return ReconciliationResult.Matched;
            }

            return ReconciliationResult.Divergent;
        }

        public bool MatchWithTolerance(
            Transaction transaction,
            ExternalEntry externalEntry,
            decimal tolerance)
        {
            if (transaction.Reference != externalEntry.Reference)
                return false;

            if (transaction.Date.Date != externalEntry.Date.Date)
                return false;

            var transactionMoney = new Money(transaction.Amount);
            var externalEntryMoney = new Money(externalEntry.Amount);

            return transactionMoney.Equals(externalEntryMoney, tolerance);
        }
    }
}