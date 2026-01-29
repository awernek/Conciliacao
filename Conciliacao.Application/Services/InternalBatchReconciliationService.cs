using Conciliacao.Application.Models;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Application.Services
{
    public class InternalBatchReconciliationService
    {
        private readonly IReconciliationPolicy _policy;

        public InternalBatchReconciliationService(IReconciliationPolicy policy)
        {
            _policy = policy;
        }

        public ReconciliationBatchResult Execute(
            IEnumerable<Transaction> transactions,
            IEnumerable<ExternalEntry> externalEntries)
        {
            var result = new ReconciliationBatchResult();

            var externalByReference = externalEntries
                .GroupBy(e => e.Reference)
                .ToDictionary(g => g.Key, g => g.First());

            var usedExternalReferences = new HashSet<string>();

            foreach (var transaction in transactions)
            {
                if (!externalByReference.TryGetValue(transaction.Reference, out var external))
                {
                    result.Missing.Add(transaction);
                    continue;
                }

                usedExternalReferences.Add(external.Reference);

                if (_policy.IsMatch(transaction, external))
                {
                    result.Matched.Add((transaction, external));
                }
                else
                {
                    result.Divergent.Add((transaction, external));
                }
            }

            foreach (var external in externalEntries)
            {
                if (!usedExternalReferences.Contains(external.Reference))
                {
                    result.Extra.Add(external);
                }
            }

            return result;
        }
    }
}