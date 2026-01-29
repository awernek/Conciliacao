namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class MatchedPairDto
    {
        public TransactionDto Transaction { get; set; } = default!;
        public ExternalEntryDto ExternalEntry { get; set; } = default!;
    }
}
