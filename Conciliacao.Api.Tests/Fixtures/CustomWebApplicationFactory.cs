using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Repositories;
using Conciliacao.Infrastructure.Persistence.Contexts;
using Conciliacao.Infrastructure.Persistence.Repositories;
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
            // "Testing" evita que o Program registre SqlServer; aqui registramos InMemory + repositórios
            builder.UseEnvironment("Testing");
            builder.UseSetting("DetailedErrors", "true");

            // Evita falha "Cannot open log for source '.NET Runtime'. You may not have write access."
            // (Event Log do Windows exige permissões que o processo de teste não tem)
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddDebug();
            });

            // Em ambiente "Testing" o Program não registra DbContext; registramos aqui InMemory + repositórios
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<ConciliationDbContext>(options =>
                    options.UseInMemoryDatabase(InMemoryDatabaseName));

                services.AddScoped<ITransactionRepository, TransactionRepository>();
                services.AddScoped<IExternalEntryRepository, ExternalEntryRepository>();
            });
        }
    }
}