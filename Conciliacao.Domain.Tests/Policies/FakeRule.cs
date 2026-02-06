using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies
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
