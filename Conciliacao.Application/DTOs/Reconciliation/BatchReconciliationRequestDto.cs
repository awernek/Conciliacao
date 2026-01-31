namespace Conciliacao.Application.DTOs.Reconciliation
{
    /// <summary>
    /// DTO de requisição para conciliação em lote. ClientCode é enviado na query; Transactions e ExternalEntries no body.
    /// </summary>
    public class BatchReconciliationRequestDto
    {
        public BatchReconciliationRequestDto()
        {
            Transactions = new List<TransactionDto>();
            ExternalEntries = new List<ExternalEntryDto>();
        }

        /// <summary>Chave de idempotência (opcional).</summary>
        public string? IdempotencyKey { get; set; }

        /// <summary>Lista de transações a conciliar.</summary>
        public List<TransactionDto> Transactions { get; set; }

        /// <summary>Lista de entradas externas a conciliar.</summary>
        public List<ExternalEntryDto> ExternalEntries { get; set; }
    }
}
