using Conciliacao.Api.Tests.Infrastructure;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Contexts;
using Conciliacao.Infra.Persistence;
using Conciliacao.Infra.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Conciliacao.Api.Tests.Fixtures
{
    /// <summary>
    /// Factory do host de testes da API. Configura Application, repositórios reais e EF Core InMemory
    /// para que a API funcione em testes de integração sem banco externo.
    /// </summary>
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        /// <summary>Nome do banco in-memory usado nos testes (um por factory/host).</summary>
        private const string InMemoryDatabaseName = "ConciliacaoTests";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ConciliationDbContext>));

                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddSingleton<SaveChangesCallCounter>();

                services.AddDbContext<ConciliationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("ConciliationTestDb");
                });

                services.AddScoped<TestConciliationDbContext>();
                services.AddScoped<ConciliationDbContext>(
                    sp => sp.GetRequiredService<TestConciliationDbContext>());

                // Em ambiente Testing o Program não registra repositórios nem IUnitOfWork; registramos aqui.
                services.AddScoped<TransactionRepository>();
                services.AddScoped<ITransactionRepository>(sp =>
                    new ThrowingOnTxFailTransactionRepositoryDecorator(sp.GetRequiredService<TransactionRepository>()));
                services.AddScoped<IExternalEntryRepository, ExternalEntryRepository>();
                services.AddScoped<IProcessedRequestRepository, ProcessedRequestRepository>();
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            });
        }
    }
}