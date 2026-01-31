namespace Conciliacao.Application.Requests
{
    public class ConciliationItem
    {
        public string Reference { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }
}
