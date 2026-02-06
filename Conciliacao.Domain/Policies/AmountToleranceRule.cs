using Conciliacao.Domain.Entities;
using Conciliacao.Domain.ValueObjects;

namespace Conciliacao.Domain.Policies
{
    public class AmountToleranceRule : IConciliationRule
    {
        private readonly decimal _tolerance;

        public AmountToleranceRule(decimal tolerance)
        {
            _tolerance = tolerance;
        }

        public bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry)
        {
            var transactionMoney = new Money(transaction.Amount);
            var externalMoney = new Money(externalEntry.Amount);

            return transactionMoney.Equals(externalMoney, _tolerance);
        }
    }
}
