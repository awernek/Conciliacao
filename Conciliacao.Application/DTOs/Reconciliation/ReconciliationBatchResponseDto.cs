namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class ReconciliationBatchResponseDto
    {
        public List<TransactionDto> Missing { get; set; } = new();
        public List<ExternalEntryDto> Extra { get; set; } = new();
        public List<(TransactionDto Transaction, ExternalEntryDto ExternalEntry)> Matched { get; set; } = new();
        public List<(TransactionDto Transaction, ExternalEntryDto ExternalEntry)> Divergent { get; set; } = new();
    }
}