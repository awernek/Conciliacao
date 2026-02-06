using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Application.Services
{
    /// <summary>
    /// Contrato para o serviço de aplicação de conciliação em lote.
    /// </summary>
    public interface IReconciliationAppService
    {
        Task<ReconciliationBatchResponseDto> ReconcileBatchAsync(
            Client client,
            IEnumerable<TransactionDto> transactionDtos,
            IEnumerable<ExternalEntryDto> externalEntryDtos);
    }
}
