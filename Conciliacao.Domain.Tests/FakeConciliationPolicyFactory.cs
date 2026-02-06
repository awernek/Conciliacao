using Conciliacao.Application.Factories;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Fábrica falsa de políticas de conciliação para testes, que sempre cria uma política composta com regras padrão.
    /// </summary> <remarks>
    /// Esta fábrica é usada para injetar uma política de conciliação consistente em testes, sem depender de configurações externas ou variações.
    /// A política criada é composta por regras de correspondência de referência, data e tolerância de valor, com uma tolerância fixa de 0.05.
    /// </remarks>
    public class FakeConciliationPolicyFactory : IConciliationPolicyFactory
    {
        /// <summary> Cria uma política de conciliação composta com regras de referência, data e tolerância de valor. </summary>
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
