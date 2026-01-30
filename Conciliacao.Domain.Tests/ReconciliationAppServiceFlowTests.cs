using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Factories;
using Conciliacao.Application.Services;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes de fluxo do serviço de aplicação de conciliação com factory real.
    /// </summary>
    public class ReconciliationAppServiceFlowTests
    {
        [Fact]
        public void Should_Reconcile_Batch_Correctly_For_Client_A()
        {
            // Preparar
            var factory = new ReconciliationPolicyFactory();
            var appService = new ReconciliationAppService(factory);

            var request = new ReconciliationBatchRequestDto
            {
                ClientCode = "CLIENT_A",

                Transactions = new List<TransactionDto>
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
                },

                ExternalEntries = new List<ExternalEntryDto>
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
                }
            };

            // Agir
            var result = appService.ReconcileBatch(request);

            // Verificar
            Assert.Single(result.Matched);
            Assert.Single(result.Missing);
            Assert.Empty(result.Divergent);
            Assert.Single(result.Extra);
        }
    }
}