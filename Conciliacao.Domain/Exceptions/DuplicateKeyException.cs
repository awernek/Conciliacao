namespace Conciliacao.Domain.Exceptions
{
    /// <summary>
    /// Exceção de domínio lançada quando uma operação de persistência viola uma restrição de chave única.
    /// Traduzida pela infraestrutura (UnitOfWork) para que a camada Application não dependa de EF Core.
    /// </summary>
    public class DuplicateKeyException : Exception
    {
        public DuplicateKeyException(string message)
            : base(message) { }

        public DuplicateKeyException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
