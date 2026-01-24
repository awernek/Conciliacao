using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests
{
    public class DefaultReconciliationPolicyTests
    {
        /// <summary>
        /// Verifies that the IsMatch method returns <see langword="true"/> when the transaction amount and external
        /// entry amount differ by a value within the allowed tolerance.
        /// </summary>
        /// <remarks>This test ensures that the DefaultReconciliationPolicy correctly identifies matching
        /// transactions when the difference between amounts does not exceed the specified tolerance. It uses a
        /// tolerance of 0.05 and checks that amounts differing by 0.02 are considered a match.</remarks>
        [Fact]
        public void IsMatch_Should_Return_True_When_Amount_Is_Within_Tolerance()
        {
            var policy = new DefaultReconciliationPolicy(0.05m);

            var transaction = new Transaction
            {
                Reference = "ABC123",
                Amount = 100.00m,
                Date = new DateTime(2025, 1, 10)
            };

            var externalEntry = new ExternalEntry
            {
                Reference = "ABC123",
                Amount = 99.98m,
                Date = new DateTime(2025, 1, 10)
            };

            var result = policy.IsMatch(transaction, externalEntry);

            Assert.True(result);
        }
    }
}