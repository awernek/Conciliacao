namespace Conciliacao.Domain.Exceptions
{
    /// <summary>
    /// Exceção de domínio lançada quando um cliente não possui política de conciliação configurada.
    /// </summary>
    public class ClientNotConfiguredForConciliationException : InvalidOperationException
    {
        public string ClientCode { get; }

        public ClientNotConfiguredForConciliationException(string clientCode)
            : base($"Cliente '{clientCode}' não configurado para conciliação.")
        {
            ClientCode = clientCode;
        }
    }
}
