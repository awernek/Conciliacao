using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Testes da política padrão de conciliação (DefaultReconciliationPolicy) e tolerância de valor.
    /// </summary>
    public class DefaultReconciliationPolicyTests
    {
        /// <summary>
        /// Verifica que o método IsMatch retorna <see langword="true"/> quando o valor da transação e da entrada
        /// externa diferem por um valor dentro da tolerância permitida.
        /// </summary>
        /// <remarks>Garante que a DefaultReconciliationPolicy identifica corretamente transações como
        /// correspondentes quando a diferença entre os valores não excede a tolerância. Usa tolerância 0,05
        /// e verifica que valores com diferença de 0,02 são considerados correspondentes.</remarks>
        [Fact]
        public void IsMatch_Should_Return_True_When_Amount_Is_Within_Tolerance()
        {
            // Preparar
            var policy = new DefaultReconciliationPolicy(0.05m);
            var transaction = new Transaction
            {
                Reference = "ABC123",
                Amount = 100.00m,
                Date = new DateTime(2025, 1, 10)
            };

            var externalEntry = new ExternalEntry
            {
                Reference = "ABC123",
                Amount = 99.98m,
                Date = new DateTime(2025, 1, 10)
            };

            // Agir
            var result = policy.IsMatch(transaction, externalEntry);

            // Verificar
            Assert.True(result);
        }

        /// <summary>
        /// Verifica que o método IsMatch retorna <see langword="false"/> quando o valor da transação e da entrada
        /// externa diferem mais do que a tolerância permitida.
        /// </summary>
        [Fact]
        public void IsMatch_Should_Return_False_When_Amount_Exceeds_Tolerance()
        {
            // Preparar
            var policy = new DefaultReconciliationPolicy(0.05m);
            var transaction = new Transaction
            {
                Reference = "ABC123",
                Amount = 100.00m,
                Date = new DateTime(2025, 1, 10)
            };
            var externalEntry = new ExternalEntry
            {
                Reference = "ABC123",
                Amount = 99.90m,
                Date = new DateTime(2025, 1, 10)
            };

            // Agir
            var result = policy.IsMatch(transaction, externalEntry);

            // Verificar
            Assert.False(result);
        }
    }
}