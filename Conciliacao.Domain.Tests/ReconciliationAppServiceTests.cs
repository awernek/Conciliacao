using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes do serviço de aplicação de conciliação em lote (ReconciliationAppService).
    /// </summary>
    public class ReconciliationAppServiceTests
    {
        [Fact]
        public async Task ReconcileBatchAsync_Should_Match_When_Entries_Are_Equal()
        {
            // Preparar
            var factory = new FakeReconciliationPolicyFactory();
            var transactionRepository = new FakeTransactionRepository();
            var externalEntryRepository = new FakeExternalEntryRepository();
            var service = new ReconciliationAppService(factory, transactionRepository, externalEntryRepository);

            var client = new Client { Code = "CLIENT_TEST" };
            var transactions = new List<TransactionDto>
            {
                new TransactionDto
                {
                    Reference = "ABC123",
                    Amount = 100m,
                    Date = new DateTime(2025, 1, 10)
                }
            };
            var externalEntries = new List<ExternalEntryDto>
            {
                new ExternalEntryDto
                {
                    Reference = "ABC123",
                    Amount = 100m,
                    Date = new DateTime(2025, 1, 10)
                }
            };

            // Agir
            var result = await service.ReconcileBatchAsync(client, transactions, externalEntries);

            // Verificar
            Assert.Single(result.Matched);
            Assert.Empty(result.Divergent);
            Assert.Empty(result.Missing);
            Assert.Empty(result.Extra);
        }
    }
}
