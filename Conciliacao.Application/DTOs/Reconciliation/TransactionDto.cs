namespace Conciliacao.Application.DTOs.Reconciliation
{
    public class TransactionDto
    {
        public string Reference { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public decimal Amount { get; set; }
    }
}