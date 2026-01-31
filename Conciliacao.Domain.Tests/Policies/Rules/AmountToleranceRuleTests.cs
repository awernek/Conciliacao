using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies.Rules
{
    /// <summary>
    /// Testes da regra de tolerância de valor (AmountToleranceRule).
    /// </summary>
    public class AmountToleranceRuleTests
    {
        /// <summary>
        /// Garante que a regra retorna true quando a diferença entre os valores é menor que a tolerância.
        /// </summary>
        [Fact]
        public void IsSatisfied_Should_Return_True_When_Difference_Is_Less_Than_Tolerance()
        {
            // Preparar
            var transaction = new Transaction("", 100.00m, default);
            var externalEntry = new ExternalEntry { Amount = 99.98m };
            var rule = new AmountToleranceRule(0.05m);

            // Agir
            var result = rule.IsSatisfied(transaction, externalEntry);

            // Verificar
            Assert.True(result);
        }

        /// <summary>
        /// Garante que a regra retorna true quando a diferença entre os valores é exatamente igual à tolerância.
        /// </summary>
        [Fact]
        public void IsSatisfied_Should_Return_True_When_Difference_Is_Equal_To_Tolerance()
        {
            // Preparar
            var transaction = new Transaction("", 100.00m, default);
            var externalEntry = new ExternalEntry { Amount = 99.95m };
            var rule = new AmountToleranceRule(0.05m);

            // Agir
            var result = rule.IsSatisfied(transaction, externalEntry);

            // Verificar
            Assert.True(result);
        }

        /// <summary>
        /// Garante que a regra retorna false quando a diferença entre os valores é maior que a tolerância.
        /// </summary>
        [Fact]
        public void IsSatisfied_Should_Return_False_When_Difference_Is_Greater_Than_Tolerance()
        {
            // Preparar
            var transaction = new Transaction("", 100.00m, default);
            var externalEntry = new ExternalEntry { Amount = 99.90m };
            var rule = new AmountToleranceRule(0.05m);

            // Agir
            var result = rule.IsSatisfied(transaction, externalEntry);

            // Verificar
            Assert.False(result);
        }
    }
}
