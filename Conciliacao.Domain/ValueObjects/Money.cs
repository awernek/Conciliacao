namespace Conciliacao.Domain.ValueObjects
{
    public class Money
    {
        public decimal Amount { get; set; }

        public Money(decimal amount)
        {
            Amount = amount;
        }

        public bool Equals(Money other, decimal tolerance)
        {
            if (other == null)
                return false;

            var difference = Math.Abs(Amount - other.Amount);

            return difference <= tolerance;
        }
    }
}