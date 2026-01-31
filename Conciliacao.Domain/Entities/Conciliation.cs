namespace Conciliacao.Domain.Entities
{
    public class Conciliation
    {
        public Guid Id { get; private set; }

        public string ExternalReference { get; private set; }

        public decimal Amount { get; private set; }

        public DateTime CreatedAt { get; private set; }

        protected Conciliation() { }

        public Conciliation(string externalReference, decimal amount)
        {
            Id = Guid.NewGuid();
            ExternalReference = externalReference;
            Amount = amount;
            CreatedAt = DateTime.UtcNow;
        }
    }

}
