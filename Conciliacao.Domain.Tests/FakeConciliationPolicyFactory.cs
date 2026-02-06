using Conciliacao.Application.Factories;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests
{
    public class FakeConciliationPolicyFactory : IConciliationPolicyFactory
    {
        public IReconciliationPolicy CreateFor(Client client)
        {
            return new CompositeReconciliationPolicy(new IReconciliationRule[]
            {
                new ReferenceMatchRule(),
                new DateMatchRule(),
                new AmountToleranceRule(0.05m)
            });
        }
    }
}
