using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies.Rules
{
    public class AmountToleranceRuleTests
    {
        [Fact]
        public void IsSatisfied_Should_Return_True_When_Difference_Is_Within_Tolerance()
        {
            var rule = new AmountToleranceRule(0.05m);

            var transaction = new Transaction { Amount = 100.00m };
            var external = new ExternalEntry { Amount = 99.98m };

            var result = rule.IsSatisfied(transaction, external);

            Assert.True(result);
        }

        [Fact]
        public void IsSatisfied_Should_Return_False_When_Difference_Exceeds_Tolerance()
        {
            var rule = new AmountToleranceRule(0.01m);

            var transaction = new Transaction { Amount = 100.00m };
            var external = new ExternalEntry { Amount = 99.90m };

            var result = rule.IsSatisfied(transaction, external);

            Assert.False(result);
        }
    }
}
