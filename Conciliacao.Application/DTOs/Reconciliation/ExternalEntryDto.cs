namespace Conciliacao.Application.DTOs.Reconciliation
{
    /// <summary>
    /// DTO de entrada externa para API. Propriedades com get/set públicos para serialização JSON.
    /// <see cref="Date"/> é serializado em formato ISO 8601 pelo System.Text.Json.
    /// </summary>
    public class ExternalEntryDto
    {
        public string Reference { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}