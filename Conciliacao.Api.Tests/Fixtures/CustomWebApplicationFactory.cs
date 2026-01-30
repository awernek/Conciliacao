using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Repositories;
using Conciliacao.Infrastructure.Persistence.Contexts;
using Conciliacao.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            builder.UseEnvironment("Development");

            // Application já está registrada no Program. Aqui registramos apenas a persistência:
            // DbContext com EF Core InMemory e repositórios reais, para testar integração com o fluxo real.
            builder.ConfigureServices(services =>
            {
                // Remove registros de DbContext existentes (se o Program registrar no futuro) para evitar conflito
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ConciliationDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                services.AddDbContext<ConciliationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(InMemoryDatabaseName);
                });

                services.AddScoped<ITransactionRepository, TransactionRepository>();
                services.AddScoped<IExternalEntryRepository, ExternalEntryRepository>();
            });
        }
    }
}