namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class ReconciliationBatchResponseDto
    {
        public List<MatchedPairDto> Matched { get; set; } = new();
        public List<TransactionDto> Missing { get; set; } = new();
        public List<ExternalEntryDto> Extra { get; set; } = new();
        public List<DivergenceDto> Divergent { get; set; } = new();
    }
}