namespace Conciliacao.Application.Requests
{
    public class ConciliationRequest
    {
        public List<ConciliationItem> Items { get; set; } = new();
    }
}