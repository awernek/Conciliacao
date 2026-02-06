using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Services
{
    public class SimpleReconciliationService
    {
        private readonly IReconciliationPolicy _policy;

        public SimpleReconciliationService(IReconciliationPolicy policy)
        {
            _policy = policy;
        }

        public IReadOnlyCollection<ReconciliationItem> Reconcile(
            IEnumerable<Transaction> transactions,
            IEnumerable<ExternalEntry> externalEntries)
        {
            var results = new List<ReconciliationItem>();
            var matchedExternalEntries = new HashSet<ExternalEntry>();

            var externalByReference = externalEntries
                .GroupBy(e => e.Reference)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var transaction in transactions)
            {
                if (!externalByReference.TryGetValue(transaction.Reference, out var external))
                {
                    results.Add(new ReconciliationItem(
                        transaction,
                        null,
                        ReconciliationResult.Missing));
                    continue;
                }

                if (_policy.IsMatch(transaction, external))
                {
                    results.Add(new ReconciliationItem(
                        transaction,
                        external,
                        ReconciliationResult.Matched));
                }
                else
                {
                    results.Add(new ReconciliationItem(
                        transaction,
                        external,
                        ReconciliationResult.Divergent));
                }

                matchedExternalEntries.Add(external);
            }

            foreach (var external in externalEntries)
            {
                if (!matchedExternalEntries.Contains(external))
                {
                    results.Add(new ReconciliationItem(
                        null,
                        external,
                        ReconciliationResult.Extra));
                }
            }

            return results;
        }
    }
}