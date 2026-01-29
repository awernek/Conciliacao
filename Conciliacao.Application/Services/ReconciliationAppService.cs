using Conciliacao.Application.Factories;
using Conciliacao.Application.Models;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Enums;

namespace Conciliacao.Application.Services
{
    public class ReconciliationAppService
    {
        private readonly IReconciliationPolicyFactory _factory;

        public ReconciliationAppService(IReconciliationPolicyFactory factory)
        {
            _factory = factory;
        }

        public ReconciliationResult Reconcile(
            Client client,
            Transaction transaction,
            ExternalEntry externalEntry)
        {
            var policy = _factory.CreateFor(client);

            return policy.IsMatch(transaction, externalEntry)
                ? ReconciliationResult.Matched
                : ReconciliationResult.Divergent;
        }

        public ReconciliationBatchResult ReconcileBatch(
            Client client,
            IEnumerable<Transaction> transactions,
            IEnumerable<ExternalEntry> externalEntries)
        {
            var policy = _factory.CreateFor(client);

            var service = new InternalBatchReconciliationService(policy);

            return service.Execute(transactions, externalEntries);
        }
    }
}