using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes da política composta de conciliação com regras Reference + Date + AmountTolerance.
    /// Valida que a combinação das três regras se comporta corretamente.
    /// </summary>
    public class DefaultConciliationPolicyTests
    {
        private static IConciliationPolicy CreatePolicy(decimal tolerance)
        {
            return new CompositeConciliationPolicy(new IConciliationRule[]
            {
                new ReferenceMatchRule(),
                new DateMatchRule(),
                new AmountToleranceRule(tolerance)
            });
        }

        /// <summary>
        /// Verifica que IsMatch retorna true quando referência, data e valor (dentro da tolerância) coincidem.
        /// </summary>
        [Fact]
        public void IsMatch_Should_Return_True_When_Amount_Is_Within_Tolerance()
        {
            var policy = CreatePolicy(0.05m);
            var transaction = new Transaction("", "ABC123", 100.00m, new DateTime(2025, 1, 10));
            var externalEntry = new ExternalEntry("ABC123", 99.98m, new DateTime(2025, 1, 10));

            var result = policy.IsMatch(transaction, externalEntry);

            Assert.True(result);
        }

        /// <summary>
        /// Verifica que IsMatch retorna false quando o valor excede a tolerância.
        /// </summary>
        [Fact]
        public void IsMatch_Should_Return_False_When_Amount_Exceeds_Tolerance()
        {
            var policy = CreatePolicy(0.05m);
            var transaction = new Transaction("", "ABC123", 100.00m, new DateTime(2025, 1, 10));
            var externalEntry = new ExternalEntry("ABC123", 99.90m, new DateTime(2025, 1, 10));

            var result = policy.IsMatch(transaction, externalEntry);

            Assert.False(result);
        }
    }
}
