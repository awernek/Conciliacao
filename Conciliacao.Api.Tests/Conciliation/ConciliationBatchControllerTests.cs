using Conciliacao.Api.Tests.Fixtures;
using Conciliacao.Application.DTOs.Conciliation;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Conciliacao.Api.Tests.Conciliation
{
    /// <summary>
    /// Testes de integração do endpoint POST /api/conciliation/batch (fluxo sem idempotência).
    /// </summary>
    public class ConciliationBatchControllerTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ConciliationBatchControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        #region Helpers

        private TransactionDto CreateTransactionDto(string reference, decimal amount, DateTime date)
            => new() { Reference = reference, Amount = amount, Date = date };

        private ExternalEntryDto CreateExternalEntryDto(string reference, decimal amount, DateTime date, string source = "SYSTEM")
            => new() { Reference = reference, Amount = amount, Date = date, Source = source };

        private ConciliationBatchRequestDto CreateBatchRequest(
            IEnumerable<TransactionDto> transactions,
            IEnumerable<ExternalEntryDto> externalEntries)
            => new ConciliationBatchRequestDto
            {
                Transactions = transactions.ToList(),
                ExternalEntries = externalEntries.ToList()
            };

        #endregion

        /// <summary>
        /// Garante que o POST /api/conciliation/batch retorna 200 e classifica como "matched"
        /// quando existe uma transação e uma entrada externa com mesma referência, valor e data.
        /// </summary>
        [Fact]
        public async Task POST_batch_should_return_matched_result()
        {
            // Arrange
            var clientCode = "CLIENT_A";

            var transactions = new[]
            {
                CreateTransactionDto("TX1", 100m, new DateTime(2025, 1, 10))
            };

            var externalEntries = new[]
            {
                CreateExternalEntryDto("TX1", 100m, new DateTime(2025, 1, 10))
            };

            var request = CreateBatchRequest(transactions, externalEntries);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/conciliation/batch?clientCode={clientCode}", request);

            // Assert: status code
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert: conteúdo
            var result = await response.Content.ReadFromJsonAsync<ConciliationBatchResponseDto>();
            result.Should().NotBeNull();

            // Matched
            result!.Matched.Should().ContainSingle(pair =>
                pair.Transaction!.Reference == "TX1" &&
                pair.Transaction.Amount == 100m &&
                pair.ExternalEntry!.Reference == "TX1" &&
                pair.ExternalEntry.Amount == 100m
            );

            // Outros arrays
            result.Divergent.Should().BeEmpty();
            result.Missing.Should().BeEmpty();
            result.Extra.Should().BeEmpty();
        }

        /// <summary>
        /// Garante que o POST /api/conciliation/batch retorna 200 e classifica como "divergent"
        /// quando referência e data batem, mas o valor difere além da tolerância do cliente.
        /// </summary>
        [Fact]
        public async Task POST_batch_should_return_divergent_result()
        {
            // Arrange
            var clientCode = "CLIENT_A";

            var transactions = new[]
            {
                CreateTransactionDto("TX1", 100m, new DateTime(2025, 1, 10))
            };

            var externalEntries = new[]
            {
                CreateExternalEntryDto("TX1", 90m, new DateTime(2025, 1, 10)) // Difere do Amount
            };

            var request = CreateBatchRequest(transactions, externalEntries);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/conciliation/batch?clientCode={clientCode}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ConciliationBatchResponseDto>();
            result.Should().NotBeNull();

            result!.Divergent.Should().ContainSingle(pair =>
                pair.Transaction!.Reference == "TX1" &&
                pair.Transaction.Amount == 100m &&
                pair.ExternalEntry!.Reference == "TX1" &&
                pair.ExternalEntry.Amount == 90m
            );

            result.Matched.Should().BeEmpty();
            result.Missing.Should().BeEmpty();
            result.Extra.Should().BeEmpty();
        }

        /// <summary>
        /// Garante que o POST /api/conciliation/batch retorna 200 e classifica como "missing"
        /// as transações que não possuem entrada externa correspondente.
        /// </summary>
        [Fact]
        public async Task POST_batch_should_return_missing_result()
        {
            // Arrange
            var clientCode = "CLIENT_A";

            var transactions = new[]
            {
                CreateTransactionDto("TX1", 100m, new DateTime(2025, 1, 10))
            };

            var externalEntries = Array.Empty<ExternalEntryDto>(); // Nenhum externo

            var request = CreateBatchRequest(transactions, externalEntries);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/conciliation/batch?clientCode={clientCode}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ConciliationBatchResponseDto>();
            result.Should().NotBeNull();

            result!.Missing.Should().ContainSingle(t =>
                t.Reference == "TX1" && t.Amount == 100m
            );

            result.Matched.Should().BeEmpty();
            result.Divergent.Should().BeEmpty();
            result.Extra.Should().BeEmpty();
        }

        /// <summary>
        /// Garante que o POST /api/conciliation/batch retorna 200 e classifica como "extra"
        /// as entradas externas que não possuem transação correspondente.
        /// </summary>
        [Fact]
        public async Task POST_batch_should_return_extra_result()
        {
            // Arrange
            var clientCode = "CLIENT_A";

            var transactions = Array.Empty<TransactionDto>(); // Nenhuma transação
            var externalEntries = new[]
            {
                CreateExternalEntryDto("TX1", 100m, new DateTime(2025, 1, 10))
            };

            var request = CreateBatchRequest(transactions, externalEntries);

            // Act
            var response = await _client.PostAsJsonAsync($"/api/conciliation/batch?clientCode={clientCode}", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content.ReadFromJsonAsync<ConciliationBatchResponseDto>();
            result.Should().NotBeNull();

            result!.Extra.Should().ContainSingle(e =>
                e.Reference == "TX1" && e.Amount == 100m
            );

            result.Matched.Should().BeEmpty();
            result.Divergent.Should().BeEmpty();
            result.Missing.Should().BeEmpty();
        }
    }
}
