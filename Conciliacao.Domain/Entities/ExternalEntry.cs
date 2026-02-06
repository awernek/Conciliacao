namespace Conciliacao.Domain.Entities
{
    public class ExternalEntry
    {
        public int Id { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string Reference { get; private set; } = string.Empty;
        public string Source { get; private set; } = string.Empty;

        protected ExternalEntry() { }

        public ExternalEntry(string reference, decimal amount, DateTime date, string source = "")
        {
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
            Amount = amount;
            Date = date;
            Source = source;
        }
    }
}
