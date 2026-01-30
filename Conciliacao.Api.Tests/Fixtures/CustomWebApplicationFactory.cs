using Conciliacao.Domain.Repositories;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Conciliacao.Api.Tests.Fixtures
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            // Registra implementações fake dos repositórios para que o container de DI
            // consiga resolver ReconciliationAppService (que depende de ITransactionRepository
            // e IExternalEntryRepository). Sem isso, o host de testes falha ao construir o ServiceProvider.
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ITransactionRepository, FakeTransactionRepository>();
                services.AddScoped<IExternalEntryRepository, FakeExternalEntryRepository>();
            });
        }
    }
}