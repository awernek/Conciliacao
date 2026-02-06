using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public class ReferenceMatchRule : IConciliationRule
    {
        public bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry)
        {
            return transaction.Reference == externalEntry.Reference;
        }
    }
}