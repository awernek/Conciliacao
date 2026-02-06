using Conciliacao.Domain.Repositories;

namespace Conciliacao.Domain.Tests
{
    /// <summary>
    /// Unidade de trabalho falsa para testes, que não realiza nenhuma operação ao confirmar.
    /// </summary> <remarks>
    /// Esta classe é usada para injetar uma unidade de trabalho que não tem efeitos colaterais em testes, permitindo que os testes se concentrem na lógica de negócios sem se preocupar com a persistência de dados.
    /// O método CommitAsync é implementado para simplesmente retornar uma tarefa concluída, simulando o comportamento de uma unidade de trabalho bem-sucedida sem realmente salvar nada.
    /// </remarks>
    public class FakeUnitOfWork : IUnitOfWork
    {
        public Task CommitAsync() => Task.CompletedTask;
    }
}
