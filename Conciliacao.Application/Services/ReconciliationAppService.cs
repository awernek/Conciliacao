using Conciliacao.Application.Models;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Application.Services
{
    public class ReconciliationAppService
    {
        private readonly IReconciliationPolicy _policy;

        public ReconciliationAppService(IReconciliationPolicy policy)
        {
            _policy = policy;
        }

        public ReconciliationResult Reconcile(
            Transaction transaction,
            ExternalEntry externalEntry)
        {
            if (_policy.IsMatch(transaction, externalEntry))
                return ReconciliationResult.Matched;

            return ReconciliationResult.Divergent;
        }

        public ReconciliationBatchResult ReconcileBatch(
            IEnumerable<Transaction> transactions,
            IEnumerable<ExternalEntry> externalEntries)
        {
            var result = new ReconciliationBatchResult();

            // Indexa entradas exter por Reference
            var externalByReference = externalEntries
                .GroupBy(e => e.Reference)
                .ToDictionary(g => g.Key, g => g.First());

            // Marca quais external entries já foram conciliadas
            var usedExternalReferences = new HashSet<string>();

            foreach (var transaction in transactions)
            {
                if (!externalByReference.TryGetValue(transaction.Reference, out var external))
                {
                    // Existe internamente, mas não externamente
                    result.Missing.Add(transaction);
                    continue;
                }

                // Marca como usada
                usedExternalReferences.Add(external.Reference);

                // Aplica a policy
                if (_policy.IsMatch(transaction, external))
                {
                    result.Matched.Add((transaction, external));
                }
                else
                {
                    result.Divergent.Add((transaction, external));
                }
            }

            // Tudo que sobrou do lado externo é Extra
            foreach (var externalEntrie in externalEntries)
            {
                if (!usedExternalReferences.Contains(externalEntrie.Reference))
                {
                    result.Extra.Add(externalEntrie);
                }
            }

            return result;
        }
    }
}