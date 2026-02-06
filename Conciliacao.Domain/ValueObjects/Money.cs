namespace Conciliacao.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um valor monetário.
    /// Imutável — uma vez criado, o Amount não pode ser alterado.
    /// </summary>
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; }

        public Money(decimal amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// Compara dois valores monetários com tolerância (para conciliação).
        /// </summary>
        public bool Equals(Money other, decimal tolerance)
        {
            if (other is null)
                return false;

            return Math.Abs(Amount - other.Amount) <= tolerance;
        }

        public bool Equals(Money? other) => other is not null && Amount == other.Amount;
        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => Amount.GetHashCode();

        public static bool operator ==(Money? left, Money? right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(Money? left, Money? right) => !(left == right);

        public override string ToString() => $"R$ {Amount:N2}";
    }
}