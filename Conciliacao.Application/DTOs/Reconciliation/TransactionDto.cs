using System.ComponentModel.DataAnnotations;

namespace Conciliacao.Application.DTOs.Reconciliation
{
    /// <summary>
    /// DTO de transação para API. Propriedades com get/set públicos para serialização JSON.
    /// <see cref="Date"/> é serializado em formato ISO 8601 pelo System.Text.Json.
    /// </summary>
    public class TransactionDto
    {
        [Required(ErrorMessage = "Reference é obrigatório.")]
        [StringLength(100, ErrorMessage = "Reference deve ter no máximo 100 caracteres.")]
        public string Reference { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date é obrigatório.")]
        public DateTime Date { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount deve ser maior que zero.")]
        public decimal Amount { get; set; }
    }
}