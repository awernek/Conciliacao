using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Application.Mappers
{
    public static class ReconciliationMapper
    {
        public static Transaction ToEntity(TransactionDto dto)
            => new Transaction("", dto.Reference, dto.Amount, dto.Date);

        public static ExternalEntry ToEntity(ExternalEntryDto dto)
            => new ExternalEntry(dto.Reference, dto.Amount, dto.Date);

        public static TransactionDto ToDto(Transaction entity)
            => new TransactionDto
            {
                Reference = entity.Reference,
                Amount = entity.Amount,
                Date = entity.Date
            };

        public static ExternalEntryDto ToDto(ExternalEntry entity)
            => new ExternalEntryDto
            {
                Reference = entity.Reference,
                Amount = entity.Amount,
                Date = entity.Date
            };
    }
}
