namespace Conciliacao.Application.DTOs.Conciliation
{
    public class MatchedPairDto
    {
        public TransactionDto Transaction { get; set; } = default!;
        public ExternalEntryDto ExternalEntry { get; set; } = default!;
    }
}
