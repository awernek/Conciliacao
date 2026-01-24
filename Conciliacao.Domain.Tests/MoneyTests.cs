using Conciliacao.Domain.ValueObjects;

namespace Conciliacao.Domain.Tests
{
    public class MoneyTests
    {
        /// <summary>
        /// Verifies that the Equals method returns <see langword="true"/> when the difference between two Money values
        /// is within the specified tolerance.
        /// </summary>
        /// <remarks>This test ensures that small differences within the tolerance are considered equal,
        /// validating the correct behavior of the Money.Equals overload that accepts a tolerance parameter.</remarks>
        [Fact]
        public void Equals_Should_Return_True_When_Difference_Is_Within_Tolerance()
        {
            // Arrange
            var money1 = new Money(100.00m);
            var money2 = new Money(99.98m);
            var tolerance = 0.05m;

            // Act
            var result = money1.Equals(money2, tolerance);

            // Assert
            Assert.True(result);
        }

        /// <summary>
        /// Verifies that the Equals method returns <see langword="false"/> when the difference between two Money values
        /// exceeds the specified tolerance.
        /// </summary>
        /// <remarks>This unit test ensures that the Money.Equals(Money, decimal) method correctly
        /// identifies values as unequal when their difference is greater than the allowed tolerance. It helps validate
        /// the method's behavior for edge cases involving precision and comparison thresholds.</remarks>
        [Fact]
        public void Equals_Should_Return_False_When_Difference_Is_Greater_Than_Tolerance()
        {
            // Arrange
            var money1 = new Money(100.00m);
            var money2 = new Money(99.90m);
            var tolerance = 0.05m;

            // Act
            var result = money1.Equals(money2, tolerance);

            // Assert
            Assert.False(result);
        }
    }
}
