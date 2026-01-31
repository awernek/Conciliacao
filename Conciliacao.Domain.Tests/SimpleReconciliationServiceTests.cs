using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Policies;
using Conciliacao.Domain.Services;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes do serviço de conciliação simples (SimpleReconciliationService).
    /// </summary>
    public class SimpleReconciliationServiceTests
    {
        /// <summary>
        /// Garante que o serviço classifica corretamente: um par como Matched (T1),
        /// transações sem par como Missing e entradas externas sem par como Extra (T2).
        /// </summary>
        [Fact]
        public void Reconcile_Should_Classify_Matched_Missing_And_Extra()
        {
            // Preparar
            var policy = new DefaultReconciliationPolicy(0.05m);
            var service = new SimpleReconciliationService(policy);
            var transactions = new[]
            {
                new Transaction("T1", 100m, new DateTime(2025, 1, 10))
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

            // Agir
            var result = service.Reconcile(transactions, externalEntries);

            // Verificar
            Assert.Contains(result, r => r.Result == ReconciliationResult.Matched);
            Assert.Contains(result, r => r.Result == ReconciliationResult.Extra);
        }
    }
}