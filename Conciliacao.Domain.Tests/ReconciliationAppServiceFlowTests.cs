using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Factories;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes de fluxo do serviço de aplicação de conciliação com factory real.
    /// </summary>
    public class ReconciliationAppServiceFlowTests
    {
        [Fact]
        public async Task Should_Reconcile_Batch_Correctly_For_Client_A()
        {
            // Preparar
            var factory = new ReconciliationPolicyFactory();
            var transactionRepository = new FakeTransactionRepository();
            var externalEntryRepository = new FakeExternalEntryRepository();
            var appService = new ReconciliationAppService(factory, transactionRepository, externalEntryRepository);

            var client = new Client { Code = "CLIENT_A" };
            var transactions = new List<TransactionDto>
            {
                new TransactionDto
                {
                    Reference = "TX1",
                    Amount = 100m,
                    Date = new DateTime(2025, 1, 10)
                },
                new TransactionDto
                {
                    Reference = "TX2",
                    Amount = 200m,
                    Date = new DateTime(2025, 1, 10)
                }
            };
            var externalEntries = new List<ExternalEntryDto>
            {
                new ExternalEntryDto
                {
                    Reference = "TX1",
                    Amount = 99.98m,
                    Date = new DateTime(2025, 1, 10)
                },
                new ExternalEntryDto
                {
                    Reference = "TX3",
                    Amount = 300m,
                    Date = new DateTime(2025, 1, 10)
                }
            };

            // Agir
            var result = await appService.ReconcileBatchAsync(client, transactions, externalEntries);

            // Verificar
            Assert.Single(result.Matched);
            Assert.Single(result.Missing);
            Assert.Empty(result.Divergent);
            Assert.Single(result.Extra);
        }
    }
}