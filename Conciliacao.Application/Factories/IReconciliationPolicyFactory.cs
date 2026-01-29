using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Application.Factories
{
    public interface IReconciliationPolicyFactory
    {
        IReconciliationPolicy CreateFor(Client client);
    }
}