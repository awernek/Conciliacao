using Conciliacao.Application.Factories;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Repositories;
using Conciliacao.Infra.Contexts;
using Conciliacao.Infra.Persistence;
using Conciliacao.Infra.Repositories;
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
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IProcessedRequestRepository, ProcessedRequestRepository>();
}

// Application
builder.Services.AddScoped<IConciliationPolicyFactory, ConciliationPolicyFactory>();
builder.Services.AddScoped<IConciliationBatchService, ConciliationBatchService>();
builder.Services.AddScoped<IConciliationService, ConciliationService>();

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
