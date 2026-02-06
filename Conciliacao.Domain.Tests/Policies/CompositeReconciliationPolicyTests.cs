using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies
{
    /// <summary>
    /// Testes da política composta de conciliação (CompositeReconciliationPolicy).
    /// </summary>
    public class CompositeReconciliationPolicyTests
    {
        /// <summary>
        /// Garante que IsMatch retorna true quando todas as regras da política composta são satisfeitas.
        /// </summary>
        [Fact]
        public void IsMatch_Should_Return_True_When_All_Rules_Are_Satisfied()
        {
            // Preparar
            var rules = new List<IReconciliationRule>
            {
                new FakeRule(true),
                new FakeRule(true),
                new FakeRule(true)
            };
            var policy = new CompositeReconciliationPolicy(rules);
            var transaction = new Transaction("", "", 0, default);
            var external = new ExternalEntry("", 0, default);

            // Agir
            var result = policy.IsMatch(transaction, external);

            // Verificar
            Assert.True(result);
        }

        /// <summary>
        /// Garante que IsMatch retorna false quando pelo menos uma regra da política composta não é satisfeita.
        /// </summary>
        [Fact]
        public void IsMatch_Should_Return_False_When_Any_Rule_Is_Not_Satisfied()
        {
            // Preparar
            var rules = new List<IReconciliationRule>
            {
                new FakeRule(true),
                new FakeRule(false),
                new FakeRule(true)
            };
            var policy = new CompositeReconciliationPolicy(rules);
            var transaction = new Transaction("", "", 0, default);
            var external = new ExternalEntry("", 0, default);

            // Agir
            var result = policy.IsMatch(transaction, external);

            // Verificar
            Assert.False(result);
        }
    }
}