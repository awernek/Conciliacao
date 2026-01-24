using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Policies;
using Conciliacao.Domain.Services;

namespace Conciliacao.Domain.Tests
{
    public class SimpleReconciliationBatchTests
    {
        [Fact]
        public void Reconcile_Should_Classify_Matched_Missing_And_Extra()
        {
            var policy = new DefaultReconciliationPolicy(0.05m);
            var service = new SimpleReconciliationService(policy);

            var transactions = new[]
            {
                new Transaction
                {
                    Reference = "T1",
                    Amount = 100m,
                    Date = new DateTime(2025, 1, 10)
                }
            };

            var externalEntries = new[]
            {
                new ExternalEntry
                {
                    Reference = "T1",
                    Amount = 100m,
                    Date = new DateTime(2025, 1, 10)
                },
                new ExternalEntry
                {
                    Reference = "T2",
                    Amount = 50m,
                    Date = new DateTime(2025, 1, 10)
                }
            };

            var result = service.Reconcile(transactions, externalEntries);

            Assert.Contains(result, r => r.Result == ReconciliationResult.Matched);
            Assert.Contains(result, r => r.Result == ReconciliationResult.Extra);
        }
    }
}