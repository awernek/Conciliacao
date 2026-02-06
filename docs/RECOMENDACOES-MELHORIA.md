# Recomendações de Melhoria — Sistema de Conciliação Financeira

> Revisão técnica completa do projeto. Cada item inclui o problema, onde está no código, por que importa e como corrigir.

---

## Sumário

| # | Prioridade | Problema | Camada |
|---|-----------|----------|--------|
| 1 | 🔴 Alta | Application referencia Infra diretamente | Application |
| 2 | 🔴 Alta | `catch` genérico engole exceções sem log | API |
| 3 | 🔴 Alta | `FakeRule` no projeto de produção | Domain |
| 4 | 🟡 Média | `Money` Value Object é mutável | Domain |
| 5 | 🟡 Média | `ExternalEntry` sem encapsulamento | Domain |
| 6 | 🟡 Média | DTOs duplicados e não utilizados | Application |
| 7 | 🟡 Média | `SimpleReconciliationService` não detecta Divergent | Domain |
| 8 | 🟡 Média | Conversão DTO → Entidade dentro do Request | Application |
| 9 | 🟡 Média | Validação de input ausente | API |
| 10 | 🟢 Baixa | `ReconciliationController` não usa interface | API |
| 11 | 🟢 Baixa | `async` sem `await` nos repositórios | Infra |
| 12 | 🟢 Baixa | `ProcessedRequest` sem namespace | Domain |
| 13 | 🟢 Baixa | `UnitOfWork.cs` duplicado e não utilizado | Infra |
| 14 | 🟢 Baixa | Configuração inline da entidade `Conciliation` | Infra |
| 15 | 🟢 Baixa | Namespaces inconsistentes na Infra | Infra |

---

## 🔴 Prioridade Alta

### 1. Application referencia Infra diretamente

**Onde:** `Conciliacao.Application.csproj` e `ReconciliationAppService.cs`

**Problema:**

```xml
<!-- Conciliacao.Application.csproj -->
<ProjectReference Include="..\Conciliacao.Infra\Conciliacao.Infra.csproj" />
```

```csharp
// ReconciliationAppService.cs
using Conciliacao.Infrastructure.Persistence.Repositories; // ❌ importa da Infra
```

Em Clean Architecture, a camada Application **não deveria conhecer** a camada Infrastructure. A direção de dependência correta é:

```
API → Application → Domain ← Infra
         ↑                      ↑
         └───── API registra ───┘
```

Application deveria depender apenas das **interfaces** definidas no Domain (`ITransactionRepository`, `IExternalEntryRepository`, etc.).

**Por que importa:** Viola o Dependency Inversion Principle (DIP). Se trocar o banco ou o ORM, a camada Application também precisa ser alterada — exatamente o oposto do que Clean Architecture propõe.

**Como corrigir:**

1. Remover a referência à Infra do `.csproj` da Application:

```xml
<!-- Conciliacao.Application.csproj — REMOVER esta linha -->
<ProjectReference Include="..\Conciliacao.Infra\Conciliacao.Infra.csproj" />
```

2. Remover o `using` da Infra no `ReconciliationAppService.cs`:

```csharp
// REMOVER:
using Conciliacao.Infrastructure.Persistence.Repositories;
```

3. Verificar se há algum tipo concreto da Infra sendo usado na Application. Se houver, substituir pela interface correspondente do Domain.

---

### 2. `catch` genérico engole exceções sem log

**Onde:** `ReconciliationController.cs` (linha 29) e `ConciliationController.cs` (linha 49)

**Problema:**

```csharp
// ReconciliationController.cs
catch
{
    return StatusCode(StatusCodes.Status500InternalServerError);
}

// ConciliationController.cs
catch
{
    return StatusCode(StatusCodes.Status500InternalServerError);
}
```

O `catch` sem tipo e sem logging:
- **Engole** qualquer exceção silenciosamente
- Retorna 500 sem **nenhuma informação** sobre o que deu errado
- Torna **impossível debugar** problemas em produção
- Esconde bugs que poderiam ser corrigidos

**Por que importa:** Em produção, quando algo der errado, você verá apenas "500 Internal Server Error" sem nenhuma pista do que aconteceu. Sem logs, a única forma de investigar é tentar reproduzir o problema — o que pode ser impossível.

**Como corrigir:**

```csharp
// ReconciliationController.cs
public class ReconciliationController : ControllerBase
{
    private readonly ReconciliationAppService _appService;
    private readonly ILogger<ReconciliationController> _logger; // ← adicionar

    public ReconciliationController(
        ReconciliationAppService appService,
        ILogger<ReconciliationController> logger) // ← adicionar
    {
        _appService = appService;
        _logger = logger;
    }

    [HttpPost("batch")]
    public async Task<ActionResult<ReconciliationBatchResponseDto>> ReconcileBatch(...)
    {
        try
        {
            // ... código existente
        }
        catch (Exception ex) // ← capturar a exceção
        {
            _logger.LogError(ex, "Erro ao processar conciliação batch para cliente {ClientCode}", clientCode);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { error = "Erro interno ao processar a conciliação." });
        }
    }
}
```

Fazer o mesmo para `ConciliationController`. O `ILogger` já é registrado automaticamente pelo ASP.NET Core.

---

### 3. `FakeRule` no projeto de produção

**Onde:** `Conciliacao.Domain/Policies/FakeRule.cs`

**Problema:**

```csharp
// Conciliacao.Domain/Policies/FakeRule.cs — está no projeto de PRODUÇÃO!
public class FakeRule : IReconciliationRule
{
    private readonly bool _result;

    public FakeRule(bool result)
    {
        _result = result;
    }

    public bool IsSatisfied(Transaction transaction, ExternalEntry externalEntry)
    {
        return _result;
    }
}
```

`FakeRule` é um **test double** (stub) — existe apenas para facilitar testes. Não deveria estar no projeto de domínio que vai para produção.

**Por que importa:**
- Código de teste no pacote de produção aumenta a superfície desnecessariamente
- Pode causar confusão ("isso é usado em algum fluxo real?")
- Viola o princípio de que o domínio contém apenas regras de negócio

**Como corrigir:**

1. Mover `FakeRule.cs` de `Conciliacao.Domain/Policies/` para `Conciliacao.Domain.Tests/Policies/` (ou na raiz do projeto de testes)
2. Ajustar o namespace:

```csharp
// Conciliacao.Domain.Tests/Policies/FakeRule.cs
namespace Conciliacao.Domain.Tests.Policies
{
    public class FakeRule : IReconciliationRule
    {
        // ... mesmo código
    }
}
```

3. Atualizar os `using` nos testes que usam `FakeRule` (ex: `CompositeReconciliationPolicyTests.cs`)

---

## 🟡 Prioridade Média

### 4. `Money` Value Object é mutável

**Onde:** `Conciliacao.Domain/ValueObjects/Money.cs`

**Problema:**

```csharp
public class Money
{
    public decimal Amount { get; set; } // ❌ set público — Value Object mutável!

    public Money(decimal amount)
    {
        Amount = amount;
    }

    public bool Equals(Money other, decimal tolerance)
    {
        if (other == null)
            return false;
        var difference = Math.Abs(Amount - other.Amount);
        return difference <= tolerance;
    }
}
```

Problemas identificados:
1. **`set` público** — qualquer código pode alterar o Amount depois de criado
2. **Não sobrescreve** `Equals(object)`, `GetHashCode()`, `operator==`
3. **Não é `sealed`** — pode ser herdado acidentalmente
4. **`class` ao invés de `record`** (ou struct) — Value Objects no .NET moderno podem ser `record`

**Por que importa:** Value Objects em DDD são **imutáveis por definição**. Se alguém fizer `money.Amount = 999`, o valor muda silenciosamente. Além disso, sem `Equals`/`GetHashCode`, dois `Money(100)` não são iguais quando comparados com `==` ou usados como chave de dicionário.

**Como corrigir:**

```csharp
namespace Conciliacao.Domain.ValueObjects
{
    /// <summary>
    /// Value Object que representa um valor monetário.
    /// Imutável — uma vez criado, o Amount não pode ser alterado.
    /// </summary>
    public sealed class Money : IEquatable<Money>
    {
        public decimal Amount { get; } // ← somente leitura

        public Money(decimal amount)
        {
            Amount = amount;
        }

        /// <summary>
        /// Compara dois valores monetários com tolerância (para conciliação).
        /// </summary>
        public bool Equals(Money other, decimal tolerance)
        {
            if (other is null)
                return false;

            return Math.Abs(Amount - other.Amount) <= tolerance;
        }

        // Igualdade estrutural (dois Money com mesmo Amount são iguais)
        public bool Equals(Money? other) => other is not null && Amount == other.Amount;
        public override bool Equals(object? obj) => Equals(obj as Money);
        public override int GetHashCode() => Amount.GetHashCode();

        public static bool operator ==(Money? left, Money? right)
            => left is null ? right is null : left.Equals(right);

        public static bool operator !=(Money? left, Money? right) => !(left == right);

        public override string ToString() => $"R$ {Amount:N2}";
    }
}
```

Alternativa com `record` (mais conciso no .NET 10):

```csharp
public sealed record Money(decimal Amount)
{
    public bool Equals(Money other, decimal tolerance)
        => other is not null && Math.Abs(Amount - other.Amount) <= tolerance;
}
```

---

### 5. `ExternalEntry` sem encapsulamento

**Onde:** `Conciliacao.Domain/Entities/ExternalEntry.cs`

**Problema:**

```csharp
public class ExternalEntry
{
    public int Id { get; set; }        // ❌ tudo público
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}
```

Compare com `Transaction`, que protege seus dados corretamente:

```csharp
public class Transaction
{
    public Guid Id { get; private set; }       // ✅ private set
    public decimal Amount { get; private set; }
    public DateTime Date { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string ExternalReference { get; private set; } = string.Empty;

    protected Transaction() { }  // ✅ construtor protegido para EF
    public Transaction(string externalReference, string reference, decimal amount, DateTime date)
    {
        Id = Guid.NewGuid();
        // ... validações
    }
}
```

**Por que importa:**
- Inconsistência entre entidades do mesmo domínio
- Qualquer código pode alterar `Amount`, `Reference`, etc. sem passar por validação
- Não segue o padrão DDD de encapsulamento de estado

**Como corrigir:**

```csharp
namespace Conciliacao.Domain.Entities
{
    public class ExternalEntry
    {
        public int Id { get; private set; }
        public decimal Amount { get; private set; }
        public DateTime Date { get; private set; }
        public string Reference { get; private set; } = string.Empty;
        public string Source { get; private set; } = string.Empty;

        protected ExternalEntry() { } // para EF Core

        public ExternalEntry(string reference, decimal amount, DateTime date, string source = "")
        {
            Reference = reference ?? throw new ArgumentNullException(nameof(reference));
            Amount = amount;
            Date = date;
            Source = source;
        }
    }
}
```

**Impacto:** Ao fazer essa mudança, será necessário atualizar:
- `ReconciliationMapper.ToEntity()` para usar o novo construtor
- Os testes que criam `ExternalEntry` com inicializador de objeto

---

### 6. DTOs duplicados e não utilizados

**Onde:** `Conciliacao.Application/DTOs/`

**Problema:** Existem DTOs que parecem não estar em uso ou são duplicatas:

| Arquivo | Status | Motivo |
|---------|--------|--------|
| `DTOs/ReconciliationBatchRequestDto.cs` | ❌ Duplicado | Funcionalidade idêntica a `DTOs/Reconciliation/BatchReconciliationRequestDto.cs` |
| `DTOs/ReconciliationBatchResultDto.cs` | ❌ Não usado | Nenhum controlador ou serviço o referencia |
| `DTOs/Reconciliation/DivergenceDto.cs` | ❌ Não usado | Estrutura idêntica ao `MatchedPairDto` (mesmas propriedades) |
| `DTOs/Reconciliation/ReconciliationRequestDto.cs` | ❌ Não usado | Nenhum controlador ou serviço o referencia |
| `DTOs/Reconciliation/ReconciliationResultDto.cs` | ❌ Não usado | Nenhum controlador ou serviço o referencia |

**Por que importa:**
- Código morto aumenta complexidade cognitiva ("para que serve isso?")
- Pode confundir quem está aprendendo o projeto
- DTOs duplicados causam dúvida sobre qual usar

**Como corrigir:**

1. Verificar se algum DTO está realmente em uso (buscar referências no projeto)
2. Remover os que não são usados
3. Se `DivergenceDto` era para substituir `MatchedPairDto` nos Divergent, considere consolidar em um só tipo (são idênticos)

---

### 7. `SimpleReconciliationService` não detecta Divergent

**Onde:** `Conciliacao.Domain/Services/SimpleReconciliationService.cs`

**Problema:**

```csharp
foreach (var transaction in transactions)
{
    var match = externalEntries
        .FirstOrDefault(e => _policy.IsMatch(transaction, e)); // ← usa IsMatch para BUSCAR

    if (match != null)
    {
        results.Add(new ReconciliationItem(transaction, match, ReconciliationResult.Matched));
        matchedExternalEntries.Add(match);
    }
    else
    {
        // ❌ Se IsMatch retornou false, cai aqui como Missing — não como Divergent!
        results.Add(new ReconciliationItem(transaction, null, ReconciliationResult.Missing));
    }
}
```

O problema: `FirstOrDefault(e => _policy.IsMatch(...))` procura um `ExternalEntry` onde **todas as regras são satisfeitas**. Se existe uma entry com mesma referência mas valor diferente, `IsMatch` retorna `false` e o `FirstOrDefault` pula essa entry. Resultado: a transação é classificada como **Missing** quando deveria ser **Divergent**.

Compare com `InternalBatchReconciliationService` que faz corretamente:

```csharp
// InternalBatchReconciliationService.cs — CORRETO
if (!externalByReference.TryGetValue(transaction.Reference, out var external))
{
    result.Missing.Add(transaction); // Sem referência = Missing
}
else if (_policy.IsMatch(transaction, external))
{
    result.Matched.Add(...);  // Regras OK = Matched
}
else
{
    result.Divergent.Add(...); // Mesma ref, regra falhou = Divergent
}
```

**Por que importa:** Bug lógico — classificações incorretas silenciosamente. Se `SimpleReconciliationService` for usado em algum fluxo, os resultados estarão errados.

**Como corrigir:**

```csharp
public IReadOnlyCollection<ReconciliationItem> Reconcile(
    IEnumerable<Transaction> transactions,
    IEnumerable<ExternalEntry> externalEntries)
{
    var results = new List<ReconciliationItem>();
    var matchedExternalEntries = new HashSet<ExternalEntry>();

    // Indexar por referência para busca eficiente
    var externalByReference = externalEntries
        .GroupBy(e => e.Reference)
        .ToDictionary(g => g.Key, g => g.First());

    foreach (var transaction in transactions)
    {
        if (!externalByReference.TryGetValue(transaction.Reference, out var external))
        {
            // Sem referência correspondente = Missing
            results.Add(new ReconciliationItem(transaction, null, ReconciliationResult.Missing));
            continue;
        }

        if (_policy.IsMatch(transaction, external))
        {
            results.Add(new ReconciliationItem(transaction, external, ReconciliationResult.Matched));
        }
        else
        {
            results.Add(new ReconciliationItem(transaction, external, ReconciliationResult.Divergent));
        }

        matchedExternalEntries.Add(external);
    }

    foreach (var external in externalEntries)
    {
        if (!matchedExternalEntries.Contains(external))
        {
            results.Add(new ReconciliationItem(null, external, ReconciliationResult.Extra));
        }
    }

    return results;
}
```

---

### 8. Conversão DTO → Entidade dentro do Request

**Onde:** `Conciliacao.Application/Requests/ConciliationRequest.cs`

**Problema:**

```csharp
using Conciliacao.Domain.Entities; // ❌ Request conhece entidade de domínio

public class ConciliationRequest
{
    public List<ConciliationItem> Items { get; set; } = new();

    public List<Transaction> ToTransactions()
    {
        return Items.Select(item =>
            new Transaction("", item.Reference, item.Amount, default) // ❌ valores vazios/default
        ).ToList();
    }
}
```

Problemas:
1. **DTO de Request conhece a entidade `Transaction`** — acoplamento errado
2. **`""` para `externalReference`** — valor vazio pode causar problemas
3. **`default` para `DateTime`** — `01/01/0001` como data é um bug silencioso

**Por que importa:** A conversão de DTO para Entidade é responsabilidade da camada de Application (Mapper ou Service), não do DTO.

**Como corrigir:**

```csharp
// ConciliationRequest.cs — apenas dados
public class ConciliationRequest
{
    public List<ConciliationItem> Items { get; set; } = new();
}

// ConciliationService.cs — conversão aqui
public async Task<ConciliationResult> ConciliateAsync(
    ConciliationRequest request, string idempotencyKey)
{
    var transactions = request.Items
        .Select(item => new Transaction(
            externalReference: string.Empty,
            reference: item.Reference,
            amount: item.Amount,
            date: DateTime.UtcNow)) // ← usar data real
        .ToList();

    // ... resto do fluxo
}
```

---

### 9. Validação de input ausente

**Onde:** Ambos os controllers e services

**Problema:** A API não valida nenhum input antes de processar:

```csharp
// ReconciliationController — aceita qualquer coisa
[HttpPost("batch")]
public async Task<ActionResult<ReconciliationBatchResponseDto>> ReconcileBatch(
    [FromQuery] string clientCode,          // pode ser null/vazio?
    [FromBody] BatchReconciliationRequestDto request)  // pode ter listas vazias?
{
    var client = new Client { Code = clientCode }; // usa clientCode direto, sem validar
    // ...
}
```

Cenários não tratados:
- `clientCode` null, vazio ou com espaços
- Body null
- Lista de `Transactions` vazia
- Lista de `ExternalEntries` vazia
- `Reference` duplicada dentro do mesmo batch
- `Amount` negativo ou zero
- `Date` no futuro ou `default(DateTime)`

**Por que importa:** Sem validação, a API pode:
- Salvar dados inválidos no banco
- Retornar exceções criptografadas como 500
- Permitir ataques ou uso indevido

**Como corrigir (opção 1 — Data Annotations):**

```csharp
public class BatchReconciliationRequestDto
{
    [Required]
    [MinLength(1)]
    public List<TransactionDto> Transactions { get; set; } = new();

    [Required]
    public List<ExternalEntryDto> ExternalEntries { get; set; } = new();
}

public class TransactionDto
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public string Reference { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Amount deve ser positivo")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime Date { get; set; }
}
```

**Como corrigir (opção 2 — FluentValidation, mais flexível):**

```csharp
// Adicionar pacote: FluentValidation.AspNetCore
public class BatchReconciliationRequestValidator
    : AbstractValidator<BatchReconciliationRequestDto>
{
    public BatchReconciliationRequestValidator()
    {
        RuleFor(x => x.Transactions)
            .NotEmpty().WithMessage("Pelo menos uma transação é obrigatória.");

        RuleForEach(x => x.Transactions).ChildRules(t =>
        {
            t.RuleFor(x => x.Reference).NotEmpty();
            t.RuleFor(x => x.Amount).GreaterThan(0);
            t.RuleFor(x => x.Date).NotEqual(default(DateTime));
        });
    }
}
```

E no controller:

```csharp
[HttpPost("batch")]
public async Task<ActionResult> ReconcileBatch(
    [FromQuery][Required][MinLength(1)] string clientCode,
    [FromBody] BatchReconciliationRequestDto request)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
    // ...
}
```

---

## 🟢 Prioridade Baixa

### 10. `ReconciliationController` não usa interface para o AppService

**Onde:** `Conciliacao.Api/Controllers/ReconciliationController.cs`

**Problema:**

```csharp
public class ReconciliationController : ControllerBase
{
    private readonly ReconciliationAppService _appService; // ❌ classe concreta
}
```

O `ConciliationController` usa `IConciliationService` (interface) — inconsistência.

**Como corrigir:** Criar `IReconciliationAppService` e injetar via interface.

---

### 11. `async` sem `await` nos repositórios

**Onde:** `ExternalEntryRepository.cs` e `TransactionRepository.cs`

**Problema:**

```csharp
// ExternalEntryRepository.cs
public async Task AddAsync(ExternalEntry externalEntry)  // ❌ async sem await
{
    _context.ExternalEntries.Add(externalEntry); // chamada síncrona
}

// TransactionRepository.cs
public async Task AddAsync(Transaction transaction)  // ❌ async sem await
{
    _context.Transactions.Add(transaction); // chamada síncrona
}
```

Gera warning CS1998: `This async method lacks 'await' operators and will run synchronously`.

**Como corrigir (opção A — remover async):**

```csharp
public Task AddAsync(ExternalEntry externalEntry)
{
    _context.ExternalEntries.Add(externalEntry);
    return Task.CompletedTask;
}
```

**Como corrigir (opção B — usar AddAsync do EF):**

```csharp
public async Task AddAsync(ExternalEntry externalEntry)
{
    await _context.ExternalEntries.AddAsync(externalEntry);
}
```

> **Nota:** `AddAsync` do EF Core só é útil quando se usa geradores de valor especiais. Para a maioria dos casos, `Add` síncrono + `Task.CompletedTask` é suficiente.

---

### 12. `ProcessedRequest` sem namespace

**Onde:** `Conciliacao.Domain/Entities/ProcessedRequest.cs`

**Problema:**

```csharp
// Sem namespace!
public class ProcessedRequest
{
    public Guid Id { get; private set; }
    // ...
}
```

Todas as outras entidades têm `namespace Conciliacao.Domain.Entities;`. `ProcessedRequest` não tem.

**Por que importa:**
- O tipo vai para o namespace global, podendo causar conflitos
- No model snapshot do EF Core, aparece como `"ProcessedRequest"` em vez de `"Conciliacao.Domain.Entities.ProcessedRequest"` — já é possível ver isso no `ConciliationDbContextModelSnapshot.cs`

**Como corrigir:**

```csharp
namespace Conciliacao.Domain.Entities
{
    public class ProcessedRequest
    {
        // ... mesmo código
    }
}
```

> **Atenção:** Após adicionar o namespace, será necessário gerar uma nova migration para que o EF Core reconheça a mudança no nome completo da entidade.

---

### 13. `UnitOfWork.cs` duplicado e não utilizado

**Onde:** `Conciliacao.Infra/Persistence/UnitOfWork.cs`

**Problema:**

Existem **duas implementações** de `IUnitOfWork`:

1. `ConciliationDbContext` (que implementa `IUnitOfWork` diretamente):
```csharp
public class ConciliationDbContext : DbContext, IUnitOfWork
{
    public async Task CommitAsync() => await SaveChangesAsync();
}
```

2. `UnitOfWork.cs` (classe separada que delega para o mesmo DbContext):
```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ConciliationDbContext _context;
    public async Task CommitAsync() => await _context.SaveChangesAsync();
}
```

No `Program.cs`, o **DbContext é registrado como `IUnitOfWork`**:

```csharp
builder.Services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ConciliationDbContext>());
```

O `UnitOfWork.cs` separado **nunca é registrado no DI** e provavelmente não é usado.

**Como corrigir:** Remover `Conciliacao.Infra/Persistence/UnitOfWork.cs` (arquivo não utilizado).

---

### 14. Configuração inline da entidade `Conciliation`

**Onde:** `Conciliacao.Infra/Contexts/ConciliationDbContext.cs`

**Problema:**

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly); // ✅ aplica configs externas

    // ❌ Mas Conciliation é configurada inline:
    modelBuilder.Entity<Conciliation>(entity =>
    {
        entity.HasKey(x => x.Id);
        entity.Property(x => x.ExternalReference).IsRequired().HasMaxLength(100);
        entity.Property(x => x.Amount).HasPrecision(18, 2);
        entity.HasIndex(x => x.ExternalReference).IsUnique();
    });

    modelBuilder.Entity<ProcessedRequest>(entity =>
    {
        // ... também inline
    });
}
```

As outras entidades (`Transaction`, `ExternalEntry`, `ProcessedRequest`) têm classes `IEntityTypeConfiguration<T>` separadas na pasta `Configurations/`. A entidade `Conciliation` não segue esse padrão.

**Como corrigir:** Criar `ConciliationConfiguration.cs` na pasta `Configurations/` e remover a configuração inline.

> **Nota:** `ProcessedRequest` tem configuração duplicada — está tanto inline no `OnModelCreating` quanto em `ProcessedRequestConfiguration.cs`. Remover a inline.

---

### 15. Namespaces inconsistentes na Infra

**Onde:** Projeto `Conciliacao.Infra`

**Problema:**

O projeto se chama `Conciliacao.Infra`, mas os namespaces usam nomes diferentes:

| Arquivo | Namespace |
|---------|----------|
| `ExternalEntryRepository.cs` | `Conciliacao.Infra.Repositories` |
| `TransactionRepository.cs` | `Conciliacao.Infrastructure.Persistence.Repositories` |
| `ProcessedRequestRepository.cs` | `Conciliacao.Infrastructure.Persistence.Repositories` |
| `ConciliationDbContext.cs` | `Conciliacao.Infrastructure.Persistence.Contexts` |
| `ExternalEntryConfiguration.cs` | `Conciliacao.Infrastructure.Persistence.Configurations` |

Mistura de `Conciliacao.Infra` com `Conciliacao.Infrastructure.Persistence`.

**Como corrigir:** Padronizar todos para `Conciliacao.Infra.*` (que é o nome real do projeto) ou para `Conciliacao.Infrastructure.*` — mas escolher um e usar consistentemente.

---

## Melhorias Adicionais (Sugestões)

Estas não são problemas, mas sugestões para evoluir o projeto:

### A. Adicionar Logging estruturado

Usar `ILogger<T>` nos services para rastrear o fluxo:

```csharp
_logger.LogInformation("Iniciando conciliação batch para cliente {ClientCode} com {TxCount} transações",
    client.Code, transactions.Count);
```

### B. Adicionar Health Check para o banco

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddSqlServer(connectionString);

app.MapHealthChecks("/health");
```

### C. Adicionar paginação no batch

Para lotes muito grandes, considere limitar o tamanho máximo ou paginar:

```csharp
if (request.Transactions.Count > 10_000)
    return BadRequest("Máximo de 10.000 transações por lote.");
```

### D. Considerar usar `CancellationToken`

Os métodos assíncronos não propagam `CancellationToken`:

```csharp
// Atual:
public async Task<ReconciliationBatchResponseDto> ReconcileBatchAsync(...)

// Recomendado:
public async Task<ReconciliationBatchResponseDto> ReconcileBatchAsync(
    ..., CancellationToken cancellationToken = default)
```

### E. Carregar configurações dos clientes do banco

Hoje as regras por cliente estão hardcoded na `ReconciliationPolicyFactory`. Para adicionar um novo cliente, precisa alterar código e fazer deploy. Considere carregar as configurações do banco de dados ou de `appsettings.json`.

---

## Pontos Positivos do Projeto

Para terminar com o que está **bom** (porque tem muito coisa boa!):

- ✅ **Clean Architecture** bem aplicada entre as camadas
- ✅ **Strategy + Composite** para políticas — padrão extensível e elegante
- ✅ **Unit of Work** com commit atômico (tudo ou nada)
- ✅ **Idempotência** correta com UNIQUE constraint no banco (não usa "check-then-insert")
- ✅ **Testes** cobrem os 4 resultados (Matched, Divergent, Missing, Extra)
- ✅ **Teste de rollback** verifica que nada é salvo em caso de erro
- ✅ **Teste de single-commit** garante que o UoW faz apenas um `SaveChanges` por lote
- ✅ **Decorator** para simular falhas em testes — boa prática
- ✅ **Separação clara** entre fluxo batch e fluxo idempotente
- ✅ **Value Object `Money`** com tolerância — abstração correta para comparação financeira
- ✅ **EF Core Configurations** separadas por entidade (padrão organizado)
- ✅ **Mapper estático** simples e direto (sem overhead de AutoMapper)

---

> **Resumo:** O projeto é sólido e demonstra bom conhecimento de DDD, Clean Architecture e padrões de design. As recomendações acima são refinamentos que levariam de **bom para excelente** em termos de qualidade profissional. Nenhuma delas é "furo grave" — são oportunidades de melhoria.
