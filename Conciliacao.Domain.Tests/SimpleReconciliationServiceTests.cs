using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Services;

namespace Conciliacao.Domain.Tests
{
    public class SimpleReconciliationServiceTests
    {
        [Fact]
        public void MatchWithTolerance_Should_Return_True_When_Difference_Is_Within_Tolerance()
        {
            var service = new SimpleReconciliationService();

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

            var tolerance = 0.05m;

            var result = service.MatchWithTolerance(transaction, externalEntry, tolerance);

            Assert.True(result);
        }
    }
}
