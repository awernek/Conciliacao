namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class DivergenceDto
    {
        public TransactionDto Transaction { get; set; } = default!;
        public ExternalEntryDto ExternalEntry { get; set; } = default!;
    }
}
