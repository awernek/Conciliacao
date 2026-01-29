using Conciliacao.Application.Factories;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests
{
    public class FakeReconciliationPolicyFactory : IReconciliationPolicyFactory
    {
        public IReconciliationPolicy CreateFor(Client client)
        {
            return new DefaultReconciliationPolicy(0.05m);
        }
    }
}