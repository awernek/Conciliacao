using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public interface IReconciliationRule
    {
        bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry);
    }
}