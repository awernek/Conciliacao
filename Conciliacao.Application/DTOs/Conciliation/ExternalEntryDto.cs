using System.ComponentModel.DataAnnotations;

namespace Conciliacao.Application.DTOs.Conciliation
{
    /// <summary>
    /// DTO de entrada externa para API de conciliação. Propriedades com get/set públicos para serialização JSON.
    /// </summary>
    public class ExternalEntryDto
    {
        [Required(ErrorMessage = "Reference é obrigatório.")]
        [StringLength(100, ErrorMessage = "Reference deve ter no máximo 100 caracteres.")]
        public string Reference { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date é obrigatório.")]
        public DateTime Date { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Amount deve ser maior que zero.")]
        public decimal Amount { get; set; }

        public string Source { get; set; } = string.Empty;
    }
}
