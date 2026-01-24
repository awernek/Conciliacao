namespace Conciliacao.Domain.Entities
{
    public class ExternalEntry
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Reference { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
    }
}
