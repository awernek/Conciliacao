using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public class DateMatchRule : IReconciliationRule
    {
        public bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry)
        {
            return transaction.Date.Date == externalEntry.Date.Date;
        }
    }
}