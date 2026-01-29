using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Domain.Rules
{
    public class ReferenceMatchRuleTests
    {
        [Fact]
        public void Should_return_true_when_references_are_equal()
        {
            // Arrange
            var transaction = new Transaction
            {
                Reference = "ABC123",
                Amount = 100m,
                Date = DateTime.Today
            };

            var externalEntry = new ExternalEntry
            {
                Reference = "ABC123",
                Amount = 100m,
                Date = DateTime.Today
            };

            var rule = new ReferenceMatchRule();

            // Act
            var result = rule.IsSatisfied(transaction, externalEntry);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_return_false_when_references_are_different()
        {
            var transaction = new Transaction
            {
                Reference = "ABC123",
                Amount = 100m,
                Date = DateTime.Today
            };
            var externalEntry = new ExternalEntry
            {
                Reference = "XYZ999",
                Amount = 100m,
                Date = DateTime.Today
            };

            var rule = new ReferenceMatchRule();

            var result = rule.IsSatisfied(transaction, externalEntry);

            Assert.False(result);
        }
    }
}