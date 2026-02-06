namespace Conciliacao.Application.DTOs.Conciliation
{
    /// <summary>
    /// DTO de resposta da conciliação em lote.
    /// Matched e Divergent usam <see cref="MatchedPairDto"/> para serialização JSON com nomes Transaction/ExternalEntry.
    /// </summary>
    public class ConciliationBatchResponseDto
    {
        public ConciliationBatchResponseDto()
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
