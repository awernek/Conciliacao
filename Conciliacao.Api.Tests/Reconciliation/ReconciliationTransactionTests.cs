using Conciliacao.Api.Tests.Fixtures;
using Conciliacao.Api.Tests.Infrastructure;
using Conciliacao.Application.DTOs.Reconciliation;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace Conciliacao.Api.Tests.Reconciliation
{
    /// <summary>
    /// Testes que garantem o uso correto de transação/UnitOfWork (um commit por lote).
    /// </summary>
    public class ReconciliationTransactionTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public ReconciliationTransactionTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        /// <summary>
        /// Garante que o UnitOfWork faz apenas um commit (uma chamada a SaveChanges) por requisição
        /// de conciliação em lote, evitando múltiplas transações desnecessárias.
        /// </summary>
        [Fact]
        public async Task Should_commit_only_once_per_batch()
        {
            var counter = _factory.Services.GetRequiredService<SaveChangesCallCounter>();
            counter.Reset();

            // Arrange
            var request = new BatchReconciliationRequestDto
            {
                Transactions =
                {
                    new TransactionDto
                    {
                        Reference = "TX1",
                        Amount = 100m,
                        Date = DateTime.Today
                    },
                    new TransactionDto
                    {
                        Reference = "TX2",
                        Amount = 200m,
                        Date = DateTime.Today
                    }
                },
                ExternalEntries =
                {
                    new ExternalEntryDto
                    {
                        Reference = "TX1",
                        Amount = 100m,
                        Date = DateTime.Today
                    }
                }
            };

            // Act
            var response = await _client.PostAsJsonAsync(
                "/api/reconciliation/batch?clientCode=CLIENT_A",
                request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert (transação única): contador compartilhado, pois o escopo da requisição já foi descartado
            counter.Count.Should().Be(1);
        }
    }
}