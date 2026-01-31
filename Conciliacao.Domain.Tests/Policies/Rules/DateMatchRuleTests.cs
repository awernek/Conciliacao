using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies.Rules
{
    /// <summary>
    /// Testes da regra de correspondência de data (DateMatchRule).
    /// </summary>
    public class DateMatchRuleTests
    {
        /// <summary>
        /// Garante que a regra retorna true quando transação e entrada externa têm a mesma data (mesmo dia),
        /// mesmo com horários diferentes.
        /// </summary>
        [Fact]
        public void IsSatisfied_Should_Return_True_When_Dates_Are_Same_Day()
        {
            // Preparar
            var rule = new DateMatchRule();
            var transaction = new Transaction("", "", 0, new DateTime(2025, 1, 10, 10, 30, 0));
            var external = new ExternalEntry { Date = new DateTime(2025, 1, 10, 18, 45, 0) };

            // Agir
            var result = rule.IsSatisfied(transaction, external);

            // Verificar
            Assert.True(result);
        }

        /// <summary>
        /// Garante que a regra retorna false quando transação e entrada externa têm datas em dias diferentes.
        /// </summary>
        [Fact]
        public void IsSatisfied_Should_Return_False_When_Dates_Are_Different()
        {
            // Preparar
            var rule = new DateMatchRule();
            var transaction = new Transaction("", "", 0, new DateTime(2025, 1, 10));
            var external = new ExternalEntry { Date = new DateTime(2025, 1, 11) };

            // Agir
            var result = rule.IsSatisfied(transaction, external);

            // Verificar
            Assert.False(result);
        }
    }

}
