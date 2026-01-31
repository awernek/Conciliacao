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
        private readonly IUnitOfWork _unitOfWork;

        public ReconciliationAppService(
            IReconciliationPolicyFactory factory,
            ITransactionRepository transactionRepository,
            IExternalEntryRepository externalEntryRepository,
            IUnitOfWork unitOfWork)
        {
            _factory = factory;
            _transactionRepository = transactionRepository;
            _externalEntryRepository = externalEntryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ReconciliationBatchResponseDto> ReconcileBatchAsync(
            Client client,
            IEnumerable<TransactionDto> transactionDtos,
            IEnumerable<ExternalEntryDto> externalEntryDtos)
        {
            // Map DTO -> Entidade usando o Mapper
            var transactions = transactionDtos.Select(ReconciliationMapper.ToEntity).ToList();
            var externalEntries = externalEntryDtos.Select(ReconciliationMapper.ToEntity).ToList();

            // Persistência (sem commit ainda — commit só ao final para permitir rollback em caso de erro)
            await _transactionRepository.AddRangeAsync(transactions);
            await _externalEntryRepository.AddRangeAsync(externalEntries);

            // Cria a policy do cliente
            var policy = _factory.CreateFor(client);

            // Conciliação
            var service = new InternalBatchReconciliationService(policy);
            var result = service.Execute(transactions, externalEntries);

            // Mapear resultado -> DTO usando o Mapper (MatchedPairDto para JSON com Transaction/ExternalEntry)
            var response = new ReconciliationBatchResponseDto
            {
                Missing = result.Missing.Select(ReconciliationMapper.ToDto).ToList(),
                Extra = result.Extra.Select(ReconciliationMapper.ToDto).ToList(),
                Matched = result.Matched
                    .Select(m => new MatchedPairDto
                    {
                        Transaction = ReconciliationMapper.ToDto(m.Transaction),
                        ExternalEntry = ReconciliationMapper.ToDto(m.ExternalEntry)
                    })
                    .ToList(),
                Divergent = result.Divergent
                    .Select(d => new MatchedPairDto
                    {
                        Transaction = ReconciliationMapper.ToDto(d.Transaction),
                        ExternalEntry = ReconciliationMapper.ToDto(d.ExternalEntry)
                    })
                    .ToList()
            };

            await _unitOfWork.CommitAsync();
            return response;
        }
    }
}