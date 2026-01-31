namespace Conciliacao.Domain.Repositories
{
    public interface IUnitOfWork
    {
        Task CommitAsync();
    }
}