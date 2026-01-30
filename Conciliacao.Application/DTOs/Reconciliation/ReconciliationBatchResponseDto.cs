namespace Conciliacao.Application.DTOs.Reconciliation
{
    /// <summary>
    /// DTO de resposta da conciliação em lote. Todas as listas são inicializadas no construtor.
    /// Matched e Divergent usam <see cref="MatchedPairDto"/> para serialização JSON com nomes Transaction/ExternalEntry.
    /// </summary>
    public class ReconciliationBatchResponseDto
    {
        public ReconciliationBatchResponseDto()
        {
            Missing = new List<TransactionDto>();
            Extra = new List<ExternalEntryDto>();
            Matched = new List<MatchedPairDto>();
            Divergent = new List<MatchedPairDto>();
        }

        public List<TransactionDto> Missing { get; set; }
        public List<ExternalEntryDto> Extra { get; set; }
        public List<MatchedPairDto> Matched { get; set; }
        public List<MatchedPairDto> Divergent { get; set; }
    }
}