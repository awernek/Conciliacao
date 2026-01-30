namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class BatchReconciliationRequestDto
    {
        public List<TransactionDto> Transactions { get; set; } = new();
        public List<ExternalEntryDto> ExternalEntries { get; set; } = new();
    }
}
