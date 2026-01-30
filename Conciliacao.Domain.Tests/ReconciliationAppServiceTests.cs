using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Tests;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes do serviço de aplicação de conciliação em lote (ReconciliationAppService).
    /// </summary>
    public class ReconciliationAppServiceTests
    {
        [Fact]
        public void ReconcileBatch_Should_Match_When_Entries_Are_Equal()
        {
            // Preparar
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

            // Agir
            var result = service.ReconcileBatch(request);

            // Verificar
            Assert.Single(result.Matched);
            Assert.Empty(result.Divergent);
            Assert.Empty(result.Missing);
            Assert.Empty(result.Extra);
        }
    }
}
