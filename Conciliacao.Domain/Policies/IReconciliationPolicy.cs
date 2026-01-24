using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public interface IReconciliationPolicy
    {
        bool IsMatch(Transaction transaction, ExternalEntry externalEntry);
    }
}
