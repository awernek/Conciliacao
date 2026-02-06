using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Exceptions;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Application.Factories
{
    /// <summary>
    /// Cria a política de conciliação por cliente.
    /// </summary>
    public class ConciliationPolicyFactory : IConciliationPolicyFactory
    {
        private const string ClientA = "CLIENT_A";
        private const string ClientB = "CLIENT_B";
        private const string ClientC = "CLIENT_C";

        /// <inheritdoc />
        public IConciliationPolicy CreateFor(Client client)
        {
            return client.Code switch
            {
                ClientA => CreatePolicyForClientA(),
                ClientB => CreatePolicyForClientB(),
                ClientC => CreatePolicyForClientC(),
                _ => throw new ClientNotConfiguredForConciliationException(client.Code)
            };
        }

        /// <summary>CLIENT_A: referência + data + tolerância de valor 0,05.</summary>
        private static IConciliationPolicy CreatePolicyForClientA()
        {
            return new CompositeConciliationPolicy(new IConciliationRule[]
            {
                new ReferenceMatchRule(),
                new DateMatchRule(),
                new AmountToleranceRule(0.05m)
            });
        }

        /// <summary>CLIENT_B: referência + data + valor exato (tolerância 0).</summary>
        private static IConciliationPolicy CreatePolicyForClientB()
        {
            return new CompositeConciliationPolicy(new IConciliationRule[]
            {
                new ReferenceMatchRule(),
                new DateMatchRule(),
                new AmountToleranceRule(0.00m)
            });
        }

        /// <summary>CLIENT_C: referência + tolerância 0,10 (sem regra de data).</summary>
        private static IConciliationPolicy CreatePolicyForClientC()
        {
            return new CompositeConciliationPolicy(new IConciliationRule[]
            {
                new ReferenceMatchRule(),
                new AmountToleranceRule(0.10m)
            });
        }
    }
}
