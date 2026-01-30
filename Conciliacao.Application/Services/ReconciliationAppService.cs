using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Factories;
using Conciliacao.Application.Mappers;
using Conciliacao.Application.Models;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infrastructure.Persistence.Repositories;

namespace Conciliacao.Application.Services
{
    public class ReconciliationAppService
    {
        private readonly IReconciliationPolicyFactory _factory;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExternalEntryRepository _externalEntryRepository;

        public ReconciliationAppService(
            IReconciliationPolicyFactory factory,
            ITransactionRepository transactionRepository,
            IExternalEntryRepository externalEntryRepository)
        {
            _factory = factory;
            _transactionRepository = transactionRepository;
            _externalEntryRepository = externalEntryRepository;
        }

        public async Task<ReconciliationBatchResponseDto> ReconcileBatchAsync(
            Client client,
            IEnumerable<TransactionDto> transactionDtos,
            IEnumerable<ExternalEntryDto> externalEntryDtos)
        {
            // Map DTO -> Entidade usando o Mapper
            var transactions = transactionDtos.Select(ReconciliationMapper.ToEntity).ToList();
            var externalEntries = externalEntryDtos.Select(ReconciliationMapper.ToEntity).ToList();

            // Persistência
            foreach (var t in transactions)
                await _transactionRepository.AddAsync(t);

            foreach (var e in externalEntries)
                await _externalEntryRepository.AddAsync(e);

            // Cria a policy do cliente
            var policy = _factory.CreateFor(client);

            // Conciliação
            var service = new InternalBatchReconciliationService(policy);
            var result = service.Execute(transactions, externalEntries);

            // Mapear resultado -> DTO usando o Mapper
            return new ReconciliationBatchResponseDto
            {
                Missing = result.Missing.Select(ReconciliationMapper.ToDto).ToList(),
                Extra = result.Extra.Select(ReconciliationMapper.ToDto).ToList(),
                Matched = result.Matched
                    .Select(m => (ReconciliationMapper.ToDto(m.Transaction), ReconciliationMapper.ToDto(m.ExternalEntry)))
                    .ToList(),
                Divergent = result.Divergent
                    .Select(d => (ReconciliationMapper.ToDto(d.Transaction), ReconciliationMapper.ToDto(d.ExternalEntry)))
                    .ToList()
            };
        }
    }
}