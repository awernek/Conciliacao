using Conciliacao.Domain.Entities;
using Conciliacao.Domain.ValueObjects;

namespace Conciliacao.Domain.Policies
{
    public class DefaultReconciliationPolicy : IReconciliationPolicy
    {
        private readonly decimal _tolerance;

        public DefaultReconciliationPolicy(decimal tolerance)
        {
            _tolerance = tolerance;
        }

        public bool IsMatch(Transaction transaction, ExternalEntry externalEntry)
        {
            if (transaction.Reference != externalEntry.Reference)
                return false;

            if (transaction.Date.Date != externalEntry.Date.Date)
                return false;

            var transactionMoney = new Money(transaction.Amount);
            var externalEntryMoney = new Money(externalEntry.Amount);

            return transactionMoney.Equals(externalEntryMoney, _tolerance);
        }
    }
}