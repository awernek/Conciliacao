using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies.Rules
{
    /// <summary>
    /// Testes da regra de correspondência de referência (ReferenceMatchRule).
    /// </summary>
    public class ReferenceMatchRuleTests
    {
        /// <summary>
        /// Garante que a regra retorna true quando a referência da transação e da entrada externa são iguais.
        /// </summary>
        [Fact]
        public void IsSatisfied_Should_Return_True_When_References_Are_Equal()
        {
            // Preparar
            var rule = new ReferenceMatchRule();
            var transaction = new Transaction("", "ABC123", 0, default);
            var external = new ExternalEntry("ABC123", 0, default);

            // Agir
            var result = rule.IsSatisfied(transaction, external);

            // Verificar
            Assert.True(result);
        }

        /// <summary>
        /// Garante que a regra retorna false quando a referência da transação e da entrada externa são diferentes.
        /// </summary>
        [Fact]
        public void IsSatisfied_Should_Return_False_When_References_Are_Different()
        {
            // Preparar
            var rule = new ReferenceMatchRule();
            var transaction = new Transaction("", "ABC123", 0, default);
            var external = new ExternalEntry("XYZ999", 0, default);

            // Agir
            var result = rule.IsSatisfied(transaction, external);

            // Verificar
            Assert.False(result);
        }
    }

}
