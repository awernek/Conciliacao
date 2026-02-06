using Conciliacao.Application.DTOs.Conciliation;
using Conciliacao.Application.Factories;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes de fluxo do serviço de aplicação de conciliação em lote com factory real.
    /// </summary>
    public class ConciliationBatchServiceFlowTests
    {
        /// <summary>
        /// Garante que, para o cliente CLIENT_A, um lote com transações e entradas externas
        /// é classificado corretamente: um matched (TX1), um missing (TX2), um extra (TX3),
        /// usando a política real com tolerância de valor.
        /// </summary>
        [Fact]
        public async Task Should_Conciliate_Batch_Correctly_For_Client_A()
        {
            // Preparar
            var factory = new ConciliationPolicyFactory();
            var transactionRepository = new FakeTransactionRepository();
            var externalEntryRepository = new FakeExternalEntryRepository();
            var unitOfWork = new FakeUnitOfWork();
            var appService = new ConciliationBatchService(factory, transactionRepository, externalEntryRepository, unitOfWork);

            var client = new Client("CLIENT_A");
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
            var result = await appService.ConciliateBatchAsync(client, transactions, externalEntries);

            // Verificar
            Assert.Single(result.Matched);
            Assert.Single(result.Missing);
            Assert.Empty(result.Divergent);
            Assert.Single(result.Extra);
        }
    }
}
