using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public class CompositeReconciliationPolicy : IReconciliationPolicy
    {
        private readonly IEnumerable<IReconciliationRule> _rules;

        public CompositeReconciliationPolicy(IEnumerable<IReconciliationRule> rules)
        {
            _rules = rules;
        }

        public bool IsMatch(Transaction transaction, ExternalEntry externalEntry)
        {
            return _rules.All(rule => rule.IsSatisfied(transaction, externalEntry));
        }
    }
}