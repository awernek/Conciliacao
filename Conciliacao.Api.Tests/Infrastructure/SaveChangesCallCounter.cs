namespace Conciliacao.Api.Tests.Infrastructure
{
    /// <summary>
    /// Contador compartilhado de chamadas a SaveChanges, para testes verificarem
    /// que o UnitOfWork é commitado uma vez por requisição (mesmo escopo já descartado).
    /// </summary>
    public class SaveChangesCallCounter
    {
        public int Count { get; private set; }

        public void Increment() => Count++;

        public void Reset() => Count = 0;
    }
}
