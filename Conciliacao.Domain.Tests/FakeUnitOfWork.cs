using Conciliacao.Domain.Repositories;

namespace Conciliacao.Domain.Tests
{
    public class FakeUnitOfWork : IUnitOfWork
    {
        public Task CommitAsync() => Task.CompletedTask;
    }
}
