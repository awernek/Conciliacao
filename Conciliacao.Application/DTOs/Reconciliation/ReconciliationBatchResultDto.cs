namespace Conciliacao.Application.DTOs
{
    public class ReconciliationBatchResultDto
    {
        public int Matched { get; set; }
        public int Divergent { get; set; }
        public int Missing { get; set; }
        public int Extra { get; set; }
    }
}