using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Application.Factories
{
    /// <summary>
    /// Factory de política de conciliação por cliente.
    /// </summary>
    public interface IConciliationPolicyFactory
    {
        IReconciliationPolicy CreateFor(Client client);
    }
}
