using Conciliacao.Application.Factories;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests
{
    public class FakeConciliationPolicyFactory : IConciliationPolicyFactory
    {
        public IConciliationPolicy CreateFor(Client client)
        {
            return new CompositeConciliationPolicy(new IConciliationRule[]
            {
                new ReferenceMatchRule(),
                new DateMatchRule(),
                new AmountToleranceRule(0.05m)
            });
        }
    }
}
