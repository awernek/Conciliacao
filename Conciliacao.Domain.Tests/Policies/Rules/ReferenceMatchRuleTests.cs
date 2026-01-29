using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies.Rules
{
    public class ReferenceMatchRuleTests
    {
        [Fact]
        public void IsSatisfied_Should_Return_True_When_References_Are_Equal()
        {
            var rule = new ReferenceMatchRule();

            var transaction = new Transaction { Reference = "ABC123" };
            var external = new ExternalEntry { Reference = "ABC123" };

            var result = rule.IsSatisfied(transaction, external);

            Assert.True(result);
        }

        [Fact]
        public void IsSatisfied_Should_Return_False_When_References_Are_Different()
        {
            var rule = new ReferenceMatchRule();

            var transaction = new Transaction { Reference = "ABC123" };
            var external = new ExternalEntry { Reference = "XYZ999" };

            var result = rule.IsSatisfied(transaction, external);

            Assert.False(result);
        }
    }

}
