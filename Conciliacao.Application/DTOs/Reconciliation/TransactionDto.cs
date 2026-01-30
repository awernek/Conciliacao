namespace Conciliacao.Application.DTOs.Reconciliation
{
    /// <summary>
    /// DTO de transação para API. Propriedades com get/set públicos para serialização JSON.
    /// <see cref="Date"/> é serializado em formato ISO 8601 pelo System.Text.Json.
    /// </summary>
    public class TransactionDto
    {
        public string Reference { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}