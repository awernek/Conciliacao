namespace Conciliacao.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public Decimal Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string Reference { get; private set; } = string.Empty;

        public Transaction()
        {
            
        }

        public Transaction(string reference, decimal amount)
        {
            Id = Guid.NewGuid();
            Amount = amount;
            Reference = reference;
        }

        public Transaction(string reference, decimal amount, DateTime date)
        {
            Id = Guid.NewGuid();
            Reference = reference;
            Amount = amount;
            Date = date;
        }
    }
}
