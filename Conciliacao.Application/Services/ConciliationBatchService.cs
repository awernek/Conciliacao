using Conciliacao.Application.DTOs.Conciliation;
using Conciliacao.Application.Factories;
using Conciliacao.Application.Mappers;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Repositories;
using Conciliacao.Domain.Services;

namespace Conciliacao.Application.Services
{
    /// <summary>
    /// Conciliação em lote: persiste transações e entradas externas, aplica política do cliente e retorna resultado (Matched, Divergent, Missing, Extra).
    /// Fluxo sem idempotência.
    /// </summary>
    public class ConciliationBatchService : IConciliationBatchService
    {
        private readonly IConciliationPolicyFactory _factory;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IExternalEntryRepository _externalEntryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ConciliationBatchService(
            IConciliationPolicyFactory factory,
            ITransactionRepository transactionRepository,
            IExternalEntryRepository externalEntryRepository,
            IUnitOfWork unitOfWork)
        {
            _factory = factory;
            _transactionRepository = transactionRepository;
            _externalEntryRepository = externalEntryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ConciliationBatchResponseDto> ConciliateBatchAsync(
            Client client,
            IEnumerable<TransactionDto> transactionDtos,
            IEnumerable<ExternalEntryDto> externalEntryDtos)
        {
            var transactions = transactionDtos.Select(ConciliationMapper.ToEntity).ToList();
            var externalEntries = externalEntryDtos.Select(ConciliationMapper.ToEntity).ToList();

            await _transactionRepository.AddRangeAsync(transactions);
            await _externalEntryRepository.AddRangeAsync(externalEntries);

            var policy = _factory.CreateFor(client);
            var service = new SimpleReconciliationService(policy);
            var items = service.Reconcile(transactions, externalEntries);

            var response = new ConciliationBatchResponseDto
            {
                Missing = items.Where(i => i.Result == ReconciliationResult.Missing)
                    .Select(i => ConciliationMapper.ToDto(i.Transaction!)).ToList(),
                Extra = items.Where(i => i.Result == ReconciliationResult.Extra)
                    .Select(i => ConciliationMapper.ToDto(i.ExternalEntry!)).ToList(),
                Matched = items.Where(i => i.Result == ReconciliationResult.Matched)
                    .Select(i => new MatchedPairDto
                    {
                        Transaction = ConciliationMapper.ToDto(i.Transaction!),
                        ExternalEntry = ConciliationMapper.ToDto(i.ExternalEntry!)
                    }).ToList(),
                Divergent = items.Where(i => i.Result == ReconciliationResult.Divergent)
                    .Select(i => new MatchedPairDto
                    {
                        Transaction = ConciliationMapper.ToDto(i.Transaction!),
                        ExternalEntry = ConciliationMapper.ToDto(i.ExternalEntry!)
                    }).ToList()
            };

            await _unitOfWork.CommitAsync();
            return response;
        }
    }
}
