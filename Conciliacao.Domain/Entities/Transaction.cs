namespace Conciliacao.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public Decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public string Reference { get; set; } = string.Empty;
    }
}
