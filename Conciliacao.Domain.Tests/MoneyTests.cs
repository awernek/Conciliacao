using Conciliacao.Domain.ValueObjects;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes do value object Money e da comparação com tolerância.
    /// </summary>
    public class MoneyTests
    {
        /// <summary>
        /// Verifica que o método Equals retorna <see langword="true"/> quando a diferença entre dois valores Money
        /// está dentro da tolerância especificada.
        /// </summary>
        /// <remarks>Garante que diferenças pequenas dentro da tolerância são consideradas iguais,
        /// validando o comportamento correto da sobrecarga Money.Equals que aceita um parâmetro de tolerância.</remarks>
        [Fact]
        public void Equals_Should_Return_True_When_Difference_Is_Within_Tolerance()
        {
            // Preparar
            var money1 = new Money(100.00m);
            var money2 = new Money(99.98m);
            var tolerance = 0.05m;

            // Agir
            var result = money1.Equals(money2, tolerance);

            // Verificar
            Assert.True(result);
        }

        /// <summary>
        /// Verifica que o método Equals retorna <see langword="false"/> quando a diferença entre dois valores Money
        /// excede a tolerância especificada.
        /// </summary>
        /// <remarks>Garante que o método Money.Equals(Money, decimal) identifica corretamente valores como
        /// diferentes quando a diferença é maior que a tolerância permitida, validando o comportamento em
        /// casos de precisão e limiares de comparação.</remarks>
        [Fact]
        public void Equals_Should_Return_False_When_Difference_Is_Greater_Than_Tolerance()
        {
            // Preparar
            var money1 = new Money(100.00m);
            var money2 = new Money(99.90m);
            var tolerance = 0.05m;

            // Agir
            var result = money1.Equals(money2, tolerance);

            // Verificar
            Assert.False(result);
        }
    }
}
