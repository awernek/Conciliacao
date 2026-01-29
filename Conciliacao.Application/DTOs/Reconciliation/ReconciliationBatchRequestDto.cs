using Conciliacao.Application.DTOs.Reconciliation;

namespace Conciliacao.Application.DTOs
{
    public class ReconciliationBatchRequestDto
    {
        public string ClientCode { get; set; } = default!;
        public IEnumerable<TransactionDto> Transactions { get; set; } = new List<TransactionDto>();
        public IEnumerable<ExternalEntryDto> ExternalEntries { get; set; } = new List<ExternalEntryDto>();
    }
}