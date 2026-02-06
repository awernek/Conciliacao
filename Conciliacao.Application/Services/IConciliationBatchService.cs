using Conciliacao.Application.DTOs.Conciliation;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Application.Services
{
    /// <summary>
    /// Serviço de aplicação para conciliação em lote (fluxo sem idempotência).
    /// </summary>
    public interface IConciliationBatchService
    {
        Task<ConciliationBatchResponseDto> ConciliateBatchAsync(
            Client client,
            IEnumerable<TransactionDto> transactionDtos,
            IEnumerable<ExternalEntryDto> externalEntryDtos);
    }
}
