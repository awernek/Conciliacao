using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Application.Factories
{
    public class ConciliationPolicyFactory : IConciliationPolicyFactory
    {
        public IConciliationPolicy CreateFor(Client client)
        {
            return client.Code switch
            {
                "CLIENT_A" => new CompositeConciliationPolicy(new IConciliationRule[]
                {
                    new ReferenceMatchRule(),
                    new DateMatchRule(),
                    new AmountToleranceRule(0.05m)
                }),

                "CLIENT_B" => new CompositeConciliationPolicy(new IConciliationRule[]
                {
                    new ReferenceMatchRule(),
                    new DateMatchRule(),
                    new AmountToleranceRule(0.00m)
                }),

                "CLIENT_C" => new CompositeConciliationPolicy(new IConciliationRule[]
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
