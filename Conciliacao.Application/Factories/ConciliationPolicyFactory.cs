using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Application.Factories
{
    public class ConciliationPolicyFactory : IConciliationPolicyFactory
    {
        public IReconciliationPolicy CreateFor(Client client)
        {
            return client.Code switch
            {
                "CLIENT_A" => new CompositeReconciliationPolicy(new IReconciliationRule[]
                {
                    new ReferenceMatchRule(),
                    new DateMatchRule(),
                    new AmountToleranceRule(0.05m)
                }),

                "CLIENT_B" => new CompositeReconciliationPolicy(new IReconciliationRule[]
                {
                    new ReferenceMatchRule(),
                    new DateMatchRule(),
                    new AmountToleranceRule(0.00m)
                }),

                "CLIENT_C" => new CompositeReconciliationPolicy(new IReconciliationRule[]
                {
                    new ReferenceMatchRule(),
                    new AmountToleranceRule(0.10m)
                }),

                _ => throw new InvalidOperationException(
                    $"Cliente '{client.Code}' não configurado para conciliação")
            };
        }
    }
}
