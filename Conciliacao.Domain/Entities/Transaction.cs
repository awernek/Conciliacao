namespace Conciliacao.Domain.Entities
{
    public class Transaction
    {
        public Guid Id { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string Reference { get; private set; } = string.Empty;
        public string ExternalReference { get; private set; } = string.Empty;

        //  Construtor protegido para o EF
        protected Transaction() { }

        //  Construtor principal do domínio
        public Transaction(
            string externalReference,
            string reference,
            decimal amount,
            DateTime date)
        {
            Id = Guid.NewGuid();

            if (string.IsNullOrWhiteSpace(externalReference))
                throw new ArgumentNullException(nameof(externalReference));

            ExternalReference = externalReference;
            Reference = reference ?? string.Empty;
            Amount = amount;
            Date = date;
        }
    }
}