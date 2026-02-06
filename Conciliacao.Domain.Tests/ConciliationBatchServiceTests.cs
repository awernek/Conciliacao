using Conciliacao.Application.DTOs.Conciliation;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes do serviço de aplicação de conciliação em lote (ConciliationBatchService).
    /// </summary>
    public class ConciliationBatchServiceTests
    {
        /// <summary>
        /// Garante que, quando transações e entradas externas têm referência, valor e data iguais,
        /// o resultado contém exatamente um par em Matched e nenhum em Divergent, Missing ou Extra.
        /// </summary>
        [Fact]
        public async Task ConciliateBatchAsync_Should_Match_When_Entries_Are_Equal()
        {
            // Preparar
            var factory = new FakeConciliationPolicyFactory();
            var transactionRepository = new FakeTransactionRepository();
            var externalEntryRepository = new FakeExternalEntryRepository();
            var unitOfWork = new FakeUnitOfWork();
            var service = new ConciliationBatchService(factory, transactionRepository, externalEntryRepository, unitOfWork);

            var client = new Client("CLIENT_TEST");
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
            var result = await service.ConciliateBatchAsync(client, transactions, externalEntries);

            // Verificar
            Assert.Single(result.Matched);
            Assert.Empty(result.Divergent);
            Assert.Empty(result.Missing);
            Assert.Empty(result.Extra);
        }
    }
}
