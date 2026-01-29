using Conciliacao.Application.DTOs.Reconciliation;

namespace Conciliacao.Application.DTOs
{
    public class ReconciliationBatchRequestDto
    {
        public string ClientCode { get; set; } = default!;
        public List<TransactionDto> Transactions { get; set; } = new();
        public List<ExternalEntryDto> ExternalEntries { get; set; } = new();
    }
}