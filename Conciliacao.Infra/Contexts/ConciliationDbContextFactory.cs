using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Conciliacao.Infra.Contexts
{
    /// <summary>
    /// Fábrica de design-time para o EF Core criar o <see cref="ConciliationDbContext"/>.
    /// Quando você roda com --startup-project Conciliacao.Api, o diretório de trabalho é a API
    /// e a connection string é lida do appsettings da API (ex.: Docker em localhost,1433).
    /// </summary>
    public class ConciliationDbContextFactory : IDesignTimeDbContextFactory<ConciliationDbContext>
    {
        public ConciliationDbContext CreateDbContext(string[] args)
        {
            // 1) Variável de ambiente
            var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");

            if (string.IsNullOrEmpty(connectionString))
            {
                // 2) appsettings da API (quando roda: dotnet ef ... --startup-project Conciliacao.Api)
                //    O EF define o diretório de trabalho como a pasta da API.
                var basePath = Directory.GetCurrentDirectory();
                var config = new ConfigurationBuilder()
                    .SetBasePath(basePath)
                    .AddJsonFile("appsettings.json", optional: true)
                    .AddJsonFile("appsettings.Development.json", optional: true)
                    .Build();
                connectionString = config.GetConnectionString("DefaultConnection");
            }

            if (string.IsNullOrEmpty(connectionString))
            {
                // 3) Fallback: LocalDb
                connectionString = "Server=(localdb)\\mssqllocaldb;Database=ConciliationDb;Trusted_Connection=True;TrustServerCertificate=True";
            }

            var optionsBuilder = new DbContextOptionsBuilder<ConciliationDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new ConciliationDbContext(optionsBuilder.Options);
        }
    }
}
