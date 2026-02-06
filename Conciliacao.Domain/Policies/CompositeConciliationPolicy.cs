using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public class CompositeConciliationPolicy : IConciliationPolicy
    {
        private readonly IEnumerable<IConciliationRule> _rules;

        public CompositeConciliationPolicy(IEnumerable<IConciliationRule> rules)
        {
            _rules = rules;
        }

        public bool IsMatch(Transaction transaction, ExternalEntry externalEntry)
        {
            return _rules.All(rule => rule.IsSatisfied(transaction, externalEntry));
        }
    }
}
