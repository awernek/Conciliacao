using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Services
{
    public class SimpleConciliationService
    {
        private readonly IConciliationPolicy _policy;

        public SimpleConciliationService(IConciliationPolicy policy)
        {
            _policy = policy;
        }

        public IReadOnlyCollection<ConciliationItem> Conciliate(
            IEnumerable<Transaction> transactions,
            IEnumerable<ExternalEntry> externalEntries)
        {
            var results = new List<ConciliationItem>();
            var matchedExternalEntries = new HashSet<ExternalEntry>();

            var externalByReference = externalEntries
                .GroupBy(e => e.Reference)
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var transaction in transactions)
            {
                if (!externalByReference.TryGetValue(transaction.Reference, out var external))
                {
                    results.Add(new ConciliationItem(
                        transaction,
                        null,
                        ConciliationStatus.Missing));
                    continue;
                }

                if (_policy.IsMatch(transaction, external))
                {
                    results.Add(new ConciliationItem(
                        transaction,
                        external,
                        ConciliationStatus.Matched));
                }
                else
                {
                    results.Add(new ConciliationItem(
                        transaction,
                        external,
                        ConciliationStatus.Divergent));
                }

                matchedExternalEntries.Add(external);
            }

            foreach (var external in externalEntries)
            {
                if (!matchedExternalEntries.Contains(external))
                {
                    results.Add(new ConciliationItem(
                        null,
                        external,
                        ConciliationStatus.Extra));
                }
            }

            return results;
        }
    }
}
