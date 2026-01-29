using Conciliacao.Domain.Entities;

namespace Conciliacao.Domain.Policies
{
    public class FakeRule : IReconciliationRule
    {
        private readonly bool _result;

        public FakeRule(bool result)
        {
            _result = result;
        }

        public bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry)
        {
            return _result;
        }
    }
}
