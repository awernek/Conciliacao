using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public interface IConciliationPolicy
    {
        bool IsMatch(Transaction transaction, ExternalEntry externalEntry);
    }
}
