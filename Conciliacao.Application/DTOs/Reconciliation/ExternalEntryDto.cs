namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class ExternalEntryDto
    {
        public string Reference { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
        public string Source { get; set; } = string.Empty;
    }
}