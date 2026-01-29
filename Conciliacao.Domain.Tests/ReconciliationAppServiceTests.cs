using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Tests;

namespace Conciliacao.Application.Tests
{
    public class ReconciliationAppServiceTests
    {
        [Fact]
        public void ReconcileBatch_Should_Match_When_Entries_Are_Equal()
        {
            var factory = new FakeReconciliationPolicyFactory();
            var service = new ReconciliationAppService(factory);
            var client = new Client { Code = "CLIENT_TEST" };

            var transactions = new List<Transaction>
            {
                new Transaction
                {
                    Reference = "ABC123",
                    Amount = 100m,
                    Date = new DateTime(2025, 1, 10)
                }
            };

            var externalEntries = new List<ExternalEntry>
            {
                new ExternalEntry
                {
                    Reference = "ABC123",
                    Amount = 100m,
                    Date = new DateTime(2025, 1, 10)
                }
            };

            var result = service.ReconcileBatch(client, transactions, externalEntries);

            Assert.Single(result.Matched);
            Assert.Empty(result.Divergent);
            Assert.Empty(result.Missing);
            Assert.Empty(result.Extra);
        }
    }
}