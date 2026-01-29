using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Factories;
using Conciliacao.Application.Mappers;
using Conciliacao.Domain.Entities;

namespace Conciliacao.Application.Services
{
    public class ReconciliationAppService
    {
        private readonly IReconciliationPolicyFactory _policyFactory;

        public ReconciliationAppService(IReconciliationPolicyFactory policyFactory)
        {
            _policyFactory = policyFactory;
        }

        public ReconciliationBatchResponseDto ReconcileBatch(
            ReconciliationBatchRequestDto request)
        {
            // Validação mínima de caso de uso
            if (string.IsNullOrWhiteSpace(request.ClientCode))
                throw new ApplicationException("ClientCode is required");

            if (!request.Transactions.Any())
                throw new ApplicationException("Transactions cannot be empty");

            // Cria policy baseada no cliente
            var client = new Client { Code = request.ClientCode };
            var policy = _policyFactory.CreateFor(client);

            // Mapping DTO → Domain
            var transactions = request.Transactions
                .Select(ReconciliationMapper.ToEntity)
                .ToList();

            var externalEntries = request.ExternalEntries
                .Select(ReconciliationMapper.ToEntity)
                .ToList();

            // 4️⃣ Executa o caso de uso real
            var internalService = new InternalBatchReconciliationService(policy);
            var domainResult = internalService.Execute(transactions, externalEntries);

            // 5️⃣ Mapping Domain → DTO
            return new ReconciliationBatchResponseDto
            {
                Matched = domainResult.Matched
                    .Select(p => new MatchedPairDto
                    {
                        Transaction = ReconciliationMapper.ToDto(p.Transaction),
                        ExternalEntry = ReconciliationMapper.ToDto(p.ExternalEntry)
                    })
                    .ToList(),
                Divergent = domainResult.Divergent
                    .Select(p => new DivergenceDto
                    {
                        Transaction = ReconciliationMapper.ToDto(p.Transaction),
                        ExternalEntry = ReconciliationMapper.ToDto(p.ExternalEntry)
                    })
                    .ToList(),
                Missing = domainResult.Missing.Select(ReconciliationMapper.ToDto).ToList(),
                Extra = domainResult.Extra.Select(ReconciliationMapper.ToDto).ToList()
            };
        }
    }
}
