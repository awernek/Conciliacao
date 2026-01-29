using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Rules
{
    public class AmountToleranceRuleTests
    {
        [Fact]
        public void Should_return_true_when_difference_is_less_than_tolerance()
        {
            var transaction = new Transaction
            {
                Amount = 100.00m
            };

            var externalEntry = new ExternalEntry
            {
                Amount = 99.98m
            };

            var rule = new AmountToleranceRule(0.05m);

            var result = rule.IsSatisfied(transaction, externalEntry);

            Assert.True(result);
        }

        [Fact]
        public void Should_return_true_when_difference_is_equal_to_tolerance()
        {
            var transaction = new Transaction
            {
                Amount = 100.00m
            };

            var externalEntry = new ExternalEntry
            {
                Amount = 99.95m
            };

            var rule = new AmountToleranceRule(0.05m);

            var result = rule.IsSatisfied(transaction, externalEntry);

            Assert.True(result);
        }

        [Fact]
        public void Should_return_false_when_difference_is_greater_than_tolerance()
        {
            var transaction = new Transaction
            {
                Amount = 100.00m
            };

            var externalEntry = new ExternalEntry
            {
                Amount = 99.90m
            };

            var rule = new AmountToleranceRule(0.05m);

            var result = rule.IsSatisfied(transaction, externalEntry);

            Assert.False(result);
        }
    }
}
