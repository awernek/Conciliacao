using Conciliacao.Application.Factories;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Repositories;
using Conciliacao.Infrastructure.Persistence.Contexts;
using Conciliacao.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Banco de dados: em ambiente "Testing" a factory de testes registra InMemory + repositórios
if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' não configurada.");

    builder.Services.AddDbContext<ConciliationDbContext>(options =>
        options.UseSqlServer(connectionString));

    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<IExternalEntryRepository, ExternalEntryRepository>();
    builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ConciliationDbContext>());
}

// Application
builder.Services.AddScoped<IReconciliationPolicyFactory, ReconciliationPolicyFactory>();
builder.Services.AddScoped<ReconciliationAppService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Conciliacao API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
