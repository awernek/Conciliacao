using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public interface IConciliationRule
    {
        bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry);
    }
}
