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

            foreach (var transaction in transactions)
            {
                var match = externalEntries
                    .FirstOrDefault(e => _policy.IsMatch(transaction, e));

                if (match != null)
                {
                    results.Add(new ReconciliationItem(
                        transaction,
                        match,
                        ReconciliationResult.Matched));

                    matchedExternalEntries.Add(match);
                }
                else
                {
                    results.Add(new ReconciliationItem(
                        transaction,
                        null,
                        ReconciliationResult.Missing));
                }
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