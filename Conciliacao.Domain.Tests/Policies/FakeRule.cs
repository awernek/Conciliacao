using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Policies;

namespace Conciliacao.Domain.Tests.Policies
{
    /// <summary>
    /// Regra falsa para testes, que sempre retorna um resultado pré-definido.
    /// </summary> <param name="result">O resultado que a regra deve retornar (true ou false).</param>
    public class FakeRule : IConciliationRule
    {
        private readonly bool _result;

        public FakeRule(bool result)
        {
            _result = result;
        }

        /// <summary> Retorna o resultado pré-definido, ignorando os parâmetros de transação e entrada externa. </summary>
        public bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry)
        {
            return _result;
        }
    }
}
