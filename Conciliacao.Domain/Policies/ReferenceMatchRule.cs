using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public class ReferenceMatchRule : IReconciliationRule
    {
        public bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry)
        {
            return transaction.Reference == externalEntry.Reference;
        }
    }
}