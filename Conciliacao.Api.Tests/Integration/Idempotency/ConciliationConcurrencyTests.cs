using Conciliacao.Application.Requests;
using Conciliacao.Application.Results;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Contexts;
using Conciliacao.Infra.Persistence;
using Conciliacao.Infra.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class ConciliationServiceConcurrencyTests
{
    /// <summary>
    /// Testa que, mesmo com chamadas concorrentes usando a mesma chave de idempotência, o serviço processa a requisição apenas uma vez e retorna o mesmo resultado para ambas.
    /// Isso garante que a lógica de idempotência funciona corretamente sob condições de concorrência.
    /// </summary>
    [Fact]
    public async Task Should_Process_Request_Only_Once_When_Called_Concurrently()
    {
        // Arrange
        var provider = BuildServiceProvider();
        var idempotencyKey = Guid.NewGuid().ToString();

        // 🔥 Limpa estado anterior
        using (var cleanupScope = provider.CreateScope())
        {
            var db = cleanupScope.ServiceProvider
                .GetRequiredService<ConciliationDbContext>();

            await db.Database.ExecuteSqlRawAsync("DELETE FROM ProcessedRequests");
            await db.Database.ExecuteSqlRawAsync("DELETE FROM Transactions");
        }

        var request = new ConciliationRequest
        {
            // Ajuste conforme seu domínio
            // Exemplo:
            // Items = new[] { new ConciliationItem { Amount = 100 } }
        };

        ConciliationResult result1;
        ConciliationResult result2;

        var gate = new ManualResetEventSlim(false);

        // Act — cada chamada usa seu próprio scope/DbContext (DbContext não é thread-safe)
        var task1 = Task.Run(async () =>
        {
            gate.Wait();
            await using var scope = provider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IConciliationService>();
            return await service.ConciliateAsync(request, idempotencyKey);
        });

        var task2 = Task.Run(async () =>
        {
            gate.Wait();
            await using var scope = provider.CreateAsyncScope();
            var service = scope.ServiceProvider.GetRequiredService<IConciliationService>();
            return await service.ConciliateAsync(request, idempotencyKey);
        });

        // 🚦 Libera as duas execuções ao mesmo tempo
        gate.Set();

        result1 = await task1;
        result2 = await task2;

        // Assert — idempotência (mesmo resultado)
        Assert.Equal(result1.Success, result2.Success);
        Assert.Equal(result1.ProcessedCount, result2.ProcessedCount);

        // Assert — apenas UM ProcessedRequest persistido
        using (var verificationScope = provider.CreateScope())
        {
            var repository = verificationScope.ServiceProvider
                .GetRequiredService<IProcessedRequestRepository>();

            var processed = await repository.GetByKeyAsync(idempotencyKey);

            Assert.NotNull(processed);
        }
    }

    /// <summary>
    /// Constrói o provedor de serviços para os testes, configurando dependências e contexto de banco de dados.
    /// </summary>
    private static ServiceProvider BuildServiceProvider()
    {
        var connectionString = GetConnectionString();

        var services = new ServiceCollection();
        services.AddDbContext<ConciliationDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

        services.AddScoped<IProcessedRequestRepository, ProcessedRequestRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        // Unit of Work concreto
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IConciliationService, ConciliationService>();

        return services.BuildServiceProvider();
    }

    /// <summary>
    /// Obtém a string de conexão para o banco de dados dos testes.
    /// </summary>
    private static string GetConnectionString()
    {
        // 1) Variável de ambiente (CI, Docker, override local)
        var env = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrEmpty(env))
            return env;

        // 2) appsettings.Integration.json (copiado para o output do teste)
        var basePath = AppContext.BaseDirectory;
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.Integration.json", optional: true)
            .Build();

        var fromFile = config.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(fromFile))
            return fromFile;

        throw new InvalidOperationException(
            "Connection string não configurada. Defina ConnectionStrings__DefaultConnection no ambiente " +
            "ou em appsettings.Integration.json no projeto de testes.");
    }
}
