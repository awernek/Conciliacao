using Conciliacao.Api.Tests.Fixtures;
using Conciliacao.Application.DTOs.Reconciliation;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace Conciliacao.Api.Tests.Reconciliation
{
    public class ReconciliationControllerTests
        : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;

        public ReconciliationControllerTests(CustomWebApplicationFactory factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task POST_batch_should_return_matched_result()
        {
            // Arrange: o controller espera clientCode na query e o body como BatchReconciliationRequestDto (sem ClientCode)
            var request = new BatchReconciliationRequestDto
            {
                Transactions =
                {
                    new TransactionDto
                    {
                        Reference = "TX1",
                        Amount = 100m,
                        Date = new DateTime(2025, 1, 10)
                    }
                },
                ExternalEntries =
                {
                    new ExternalEntryDto
                    {
                        Reference = "TX1",
                        Amount = 100m,
                        Date = new DateTime(2025, 1, 10)
                    }
                }
            };

            // Act: clientCode na query; body com Transactions e ExternalEntries
            var response = await _client.PostAsJsonAsync(
                "/api/reconciliation/batch?clientCode=CLIENT_A",
                request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var result = await response.Content
                .ReadFromJsonAsync<ReconciliationBatchResponseDto>();

            result.Should().NotBeNull();
            result!.Matched.Should().HaveCount(1);
            result.Divergent.Should().BeEmpty();
            result.Missing.Should().BeEmpty();
            result.Extra.Should().BeEmpty();

            // Asserts sobre valores específicos do DTO retornado (MatchedPairDto com Transaction e ExternalEntry).
            var pair = result.Matched[0];
            pair.Transaction.Should().NotBeNull();
            pair.ExternalEntry.Should().NotBeNull();
            pair.Transaction!.Reference.Should().Be("TX1");
            pair.Transaction.Amount.Should().Be(100m);
            pair.ExternalEntry!.Reference.Should().Be("TX1");
            pair.ExternalEntry.Amount.Should().Be(100m);
        }
    }
}