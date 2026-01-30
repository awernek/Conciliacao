using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies.Rules
{
    /// <summary>
    /// Testes da regra de correspondência de data (DateMatchRule).
    /// </summary>
    public class DateMatchRuleTests
    {
        [Fact]
        public void IsSatisfied_Should_Return_True_When_Dates_Are_Same_Day()
        {
            // Preparar
            var rule = new DateMatchRule();
            var transaction = new Transaction { Date = new DateTime(2025, 1, 10, 10, 30, 0) };
            var external = new ExternalEntry { Date = new DateTime(2025, 1, 10, 18, 45, 0) };

            // Agir
            var result = rule.IsSatisfied(transaction, external);

            // Verificar
            Assert.True(result);
        }

        [Fact]
        public void IsSatisfied_Should_Return_False_When_Dates_Are_Different()
        {
            // Preparar
            var rule = new DateMatchRule();
            var transaction = new Transaction { Date = new DateTime(2025, 1, 10) };
            var external = new ExternalEntry { Date = new DateTime(2025, 1, 11) };

            // Agir
            var result = rule.IsSatisfied(transaction, external);

            // Verificar
            Assert.False(result);
        }
    }

}
