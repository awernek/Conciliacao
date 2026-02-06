using System;

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
            
            if (string.IsNullOrWhiteSpace(externalReference))
                throw new ArgumentNullException(nameof(externalReference));

            ExternalReference = externalReference;
            Amount = amount;
            CreatedAt = DateTime.UtcNow;
        }
    }
}