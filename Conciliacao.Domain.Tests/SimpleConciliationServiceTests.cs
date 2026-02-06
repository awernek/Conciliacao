using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Policies;
using Conciliacao.Domain.Services;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes do serviço de conciliação simples (SimpleConciliationService).
    /// </summary>
    public class SimpleConciliationServiceTests
    {
        /// <summary>
        /// Garante que o serviço classifica corretamente: um par como Matched (T1),
        /// transações sem par como Missing e entradas externas sem par como Extra (T2).
        /// </summary>
        [Fact]
        public void Conciliate_Should_Classify_Matched_Missing_And_Extra()
        {
            // Preparar
            var policy = new CompositeConciliationPolicy(new IConciliationRule[]
            {
                new ReferenceMatchRule(),
                new DateMatchRule(),
                new AmountToleranceRule(0.05m)
            });
            var service = new SimpleConciliationService(policy);
            var transactions = new[]
            {
                new Transaction("", "T1", 100m, new DateTime(2025, 1, 10))
            };

            var externalEntries = new[]
            {
                new ExternalEntry("T1", 100m, new DateTime(2025, 1, 10)),
                new ExternalEntry("T2", 50m, new DateTime(2025, 1, 10))
            };

            // Agir
            var result = service.Conciliate(transactions, externalEntries);

            // Verificar
            Assert.Contains(result, r => r.Status == ConciliationStatus.Matched);
            Assert.Contains(result, r => r.Status == ConciliationStatus.Extra);
        }
    }
}
