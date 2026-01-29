using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Application.Mappers
{
    public static class ReconciliationMapper
    {
        public static Transaction ToEntity(TransactionDto dto)
            => new Transaction
            {
                Reference = dto.Reference,
                Amount = dto.Amount,
                Date = dto.Date
            };

        public static ExternalEntry ToEntity(ExternalEntryDto dto)
            => new ExternalEntry
            {
                Reference = dto.Reference,
                Amount = dto.Amount,
                Date = dto.Date
            };
    }
}
