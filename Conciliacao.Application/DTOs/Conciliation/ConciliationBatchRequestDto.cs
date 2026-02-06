using System.ComponentModel.DataAnnotations;

namespace Conciliacao.Application.DTOs.Conciliation
{
    /// <summary>
    /// DTO de requisição para conciliação em lote (fluxo sem idempotência).
    /// ClientCode é enviado na query; Transactions e ExternalEntries no body.
    /// </summary>
    public class ConciliationBatchRequestDto
    {
        public ConciliationBatchRequestDto()
        {
            Transactions = new List<TransactionDto>();
            ExternalEntries = new List<ExternalEntryDto>();
        }

        /// <summary>Lista de transações a conciliar.</summary>
        [Required(ErrorMessage = "Transactions é obrigatório.")]
        public List<TransactionDto> Transactions { get; set; }

        /// <summary>Lista de entradas externas a conciliar.</summary>
        [Required(ErrorMessage = "ExternalEntries é obrigatório.")]
        public List<ExternalEntryDto> ExternalEntries { get; set; }
    }
}
