using Conciliacao.Api.Tests.Fixtures;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Infra.Contexts;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Conciliacao.Api.Tests.Reconciliation
{
    /// <summary>
    /// Testes que garantem o comportamento de rollback quando ocorre erro no processamento do lote.
    /// </summary>
    public class ReconciliationRollbackTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ReconciliationRollbackTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Garante que, quando ocorre erro durante o processamento do lote (ex.: referência TX_FAIL),
        /// a API retorna 500 e nenhum dado da requisição é persistido (rollback implícito).
        /// </summary>
        [Fact]
        public async Task Should_rollback_transaction_when_error_occurs()
        {
            // Arrange
            var request = new BatchReconciliationRequestDto
            {
                Transactions =
                {
                    new TransactionDto
                    {
                        Reference = "TX_FAIL",
                        Amount = 100m,
                        Date = DateTime.Today
                    }
                },
                ExternalEntries =
                {
                    new ExternalEntryDto
                    {
                        Reference = "TX_FAIL",
                        Amount = 100m,
                        Date = DateTime.Today
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/reconciliation/batch?clientCode=CLIENT_A",
                request);

            // Assert — API deve falhar
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

            // Assert — rollback: dados desta requisição (TX_FAIL) não devem ter sido persistidos
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider
                .GetRequiredService<ConciliationDbContext>();

            db.Transactions.Should().NotContain(t => t.Reference == "TX_FAIL");
            db.ExternalEntries.Should().NotContain(e => e.Reference == "TX_FAIL");
        }
    }
}