using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Tests;

namespace Conciliacao.Application.Tests
{
    public class ReconciliationAppServiceTests
    {
        [Fact]
        public void ReconcileBatch_Should_Match_When_Entries_Are_Equal()
        {
            // Arrange
            var factory = new FakeReconciliationPolicyFactory();
            var service = new ReconciliationAppService(factory);

            var request = new ReconciliationBatchRequestDto
            {
                ClientCode = "CLIENT_TEST",

                Transactions = new List<TransactionDto>
                {
                    new TransactionDto
                    {
                        Reference = "ABC123",
                        Amount = 100m,
                        Date = new DateTime(2025, 1, 10)
                    }
                },

                ExternalEntries = new List<ExternalEntryDto>
                {
                    new ExternalEntryDto
                    {
                        Reference = "ABC123",
                        Amount = 100m,
                        Date = new DateTime(2025, 1, 10)
                    }
                }
            };

            // Act
            var result = service.ReconcileBatch(request);

            // Assert
            Assert.Single(result.Matched);
            Assert.Empty(result.Divergent);
            Assert.Empty(result.Missing);
            Assert.Empty(result.Extra);
        }
    }
}
