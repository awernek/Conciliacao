namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class ReconciliationRequestDto
    {
        public string ClientCode { get; set; } = default!;
        public TransactionDto Transaction { get; set; } = new();
        public ExternalEntryDto ExternalEntry { get; set; } = new();
    }
}
