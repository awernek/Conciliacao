# Sistema de Conciliação Financeira

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Sistema de reconciliação automatizada de transações financeiras, construído com Domain-Driven Design (DDD) e .NET 10.

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [API REST](#-api-rest)
- [Camada de Aplicação](#-camada-de-aplicação)
- [Infraestrutura](#-infraestrutura)
- [Políticas por Cliente](#-políticas-por-cliente)
- [Idempotência e Concorrência](#-idempotência-e-concorrência)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Quick Start](#-quick-start)
- [Decisões de Design](#-decisões-de-design)
- [Documentação Adicional](#-documentação-adicional)
- [Roadmap](#-roadmap)
- [Licença](#-licença)

## 🎯 Visão Geral

O Sistema de Conciliação Financeira automatiza a reconciliação entre transações internas (ERP/Core) e lançamentos externos (bancos, gateways, APIs). Oferece **dois fluxos de API**: conciliação em lote com persistência (Reconciliation) e conciliação idempotente (Conciliation).

### Principais Funcionalidades

- ✅ **POST /api/reconciliation/batch** — Conciliação em lote: persiste transações e entradas externas, executa matching por política do cliente, retorna Matched/Divergent/Missing/Extra e faz **um único commit** no final (rollback implícito em erro).
- ✅ **POST /api/conciliation** — Conciliação **idempotente**: header `Idempotency-Key` obrigatório; requisições com a mesma chave retornam o resultado já salvo sem reprocessar (índice UNIQUE + tratamento de concorrência).
- ✅ **Persistência** — Entity Framework Core, SQL Server; repositórios (Transaction, ExternalEntry, ProcessedRequest), Unit of Work (DbContext como IUnitOfWork).
- ✅ **Política por cliente** — `IReconciliationPolicyFactory` cria a política conforme `clientCode` (CLIENT_A, CLIENT_B, CLIENT_C).
- ✅ **Testes** — Unitários (Domain, Application, Rules) e testes de API (WebApplicationFactory), incluindo transação (um commit por batch), rollback e idempotência/concorrência.
- ✅ **CI** — GitHub Actions (build + test no branch `master`).

## 🏗️ Arquitetura

### Visão de Contexto (C4 - Level 1)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      SISTEMA DE CONCILIAÇÃO                             │
├─────────────────────────────────────────────────────────────────────────┤
│    ┌──────────┐         ┌──────────────────┐         ┌───────────┐        │
│    │ Sistema  │         │                  │         │  Bancos   │        │
│    │ Interno  │────────▶│  Reconciliation │◀────────│  ERPs     │        │
│    │(ERP/Core)│         │  + Conciliation  │         │  Gateways │        │
│    └──────────┘         └──────────────────┘         └───────────┘        │
│          │                       │                         │              │
│          ▼                       ▼                         ▼              │
│    Transactions           BatchResponseDto /         ExternalEntries      │
│    ExternalEntries        ConciliationResult        ProcessedRequests     │
└─────────────────────────────────────────────────────────────────────────┘
```

### Camadas

```
┌─────────────────────────────────────────────────────────────────────────┐
│  API (Controllers)                                                       │
│  ReconciliationController  → POST /api/reconciliation/batch?clientCode=  │
│  ConciliationController    → POST /api/conciliation (Idempotency-Key)   │
│  Swagger / OpenAPI                                                       │
├─────────────────────────────────────────────────────────────────────────┤
│  Application                                                             │
│  ReconciliationAppService  → ReconcileBatchAsync (persiste + concilia + commit) │
│  ConciliationService      → ConciliateAsync (idempotente, ProcessedRequest)   │
│  InternalBatchReconciliationService, PolicyFactory, Mapper, DTOs/Requests/Results │
├─────────────────────────────────────────────────────────────────────────┤
│  Domain                                                                  │
│  Entities: Transaction, ExternalEntry, Client, ReconciliationItem, ProcessedRequest │
│  Repositories: ITransactionRepository, IExternalEntryRepository, IProcessedRequestRepository, IUnitOfWork │
│  Policies/Rules, SimpleReconciliationService, Money                      │
├─────────────────────────────────────────────────────────────────────────┤
│  Infrastructure                                                           │
│  ConciliationDbContext (IUnitOfWork), SqlServer                           │
│  TransactionRepository, ExternalEntryRepository, ProcessedRequestRepository │
│  Configurations, Migrations                                              │
└─────────────────────────────────────────────────────────────────────────┘
```

## 🌐 API REST

### 1. Conciliação em lote (persistência + matching)

| Método | Rota | Descrição |
|--------|------|-----------|
| **POST** | `/api/reconciliation/batch?clientCode={clientCode}` | Persiste transações e entradas externas, concilia com a política do cliente, retorna Matched/Divergent/Missing/Extra e faz **um commit** no final. |

- **clientCode** (query, obrigatório): define a política (ex.: CLIENT_A, CLIENT_B, CLIENT_C).
- **Body**: `BatchReconciliationRequestDto` — `transactions`, `externalEntries`; opcional `idempotencyKey`.
- **Response**: `ReconciliationBatchResponseDto` (Matched, Divergent, Missing, Extra).
- Em erro: 500; o UoW não faz commit (rollback implícito).

### 2. Conciliação idempotente

| Método | Rota | Descrição |
|--------|------|-----------|
| **POST** | `/api/conciliation` | Processa a conciliação de forma **idempotente**. Header **Idempotency-Key** obrigatório. |

- **Header**: `Idempotency-Key` (obrigatório) — ex.: GUID ou valor único por operação.
- **Body**: `ConciliationRequest` — `items` (reference, amount).
- **Response**: `ConciliationResult` — `success`, `processedCount`.
- Comportamento: primeira requisição com chave X → processa e persiste transações + ProcessedRequest; segunda requisição com a **mesma** chave X → não reprocessa, retorna o resultado já salvo (índice UNIQUE no banco; concorrência tratada com catch em violação de UNIQUE).

## 📦 Camada de Aplicação

| Componente | Descrição |
|------------|-----------|
| **ReconciliationAppService** | Recebe `Client` e listas de `TransactionDto`/`ExternalEntryDto`. Mapeia DTO→Entity, **persiste** (repositórios), obtém política via factory, executa **InternalBatchReconciliationService**, mapeia resultado para **ReconciliationBatchResponseDto**, chama **CommitAsync()** e retorna. Um único commit por request; em exceção, nada é gravado. |
| **InternalBatchReconciliationService** | Recebe entidades e `IReconciliationPolicy`. Emparelha por Reference; classifica em Matched/Divergent/Missing/Extra. Retorna **ReconciliationBatchResult**. |
| **ConciliationService** | Fluxo idempotente: monta transações a partir de **ConciliationRequest**, persiste transações e **ProcessedRequest** (idempotencyKey + resultHash). Em violação de UNIQUE (concorrência), busca ProcessedRequest pela chave e retorna **ConciliationResult** a partir do payload salvo. |
| **IReconciliationPolicyFactory** | Cria `IReconciliationPolicy` para um `Client` (CLIENT_A, CLIENT_B, CLIENT_C). |
| **ReconciliationMapper** | ToEntity/ToDto para Transaction e ExternalEntry. |

## 🗄️ Infraestrutura

- **ConciliationDbContext** — EF Core, SQL Server; implementa **IUnitOfWork** (CommitAsync = SaveChangesAsync). DbSets: Transactions, ExternalEntries, ProcessedRequests.
- **Repositórios** — TransactionRepository, ExternalEntryRepository, ProcessedRequestRepository (implementam interfaces do Domain).
- **Configurations** — mapeamento EF para Transaction, ExternalEntry, ProcessedRequest.
- **Migrations** — esquema inicial e índices (ex.: UNIQUE em ProcessedRequests.IdempotencyKey, ExternalEntry.Reference).
- Em ambiente **Testing** (ex.: testes de API), o Program.cs não registra o DbContext real; os testes usam fakes (FakeTransactionRepository, FakeExternalEntryRepository, TestConciliationDbContext, etc.).

## 👤 Políticas por Cliente

A **ReconciliationPolicyFactory** define políticas por código de cliente:

| ClientCode | Regras | Tolerância (valor) |
|------------|--------|---------------------|
| **CLIENT_A** | Reference + Date + Amount | 0,05 |
| **CLIENT_B** | Reference + Date + Amount | 0,00 (exata) |
| **CLIENT_C** | Reference + Amount (sem Date) | 0,10 |

## 🔐 Idempotência e Concorrência

- **Idempotência**: fluxo `POST /api/conciliation` com header `Idempotency-Key`. ProcessedRequest armazena chave + hash do resultado; requisições repetidas retornam o resultado já persistido (FromPayload).
- **Concorrência**: índice UNIQUE em `ProcessedRequests.IdempotencyKey`; duas requisições simultâneas com a mesma chave — uma insere, a outra recebe DbUpdateException (violação de UNIQUE), busca o ProcessedRequest pela chave e retorna o resultado já salvo.
- **Consistência no batch**: ReconciliationAppService persiste primeiro, concilia e mapeia; **só no final** chama CommitAsync(). Se algo falhar antes, nada é gravado. Controller com try/catch retorna 500 sem commit.

Detalhes: [docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md](docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md).

## 📁 Estrutura do Projeto

```
Conciliacao/
├── .github/workflows/
│   └── dotnet.yml                    # CI: build + test (master)
├── Conciliacao.Api/
│   ├── Controllers/
│   │   ├── ReconciliationController.cs   # POST /api/reconciliation/batch?clientCode=
│   │   └── ConciliationController.cs     # POST /api/conciliation (Idempotency-Key)
│   └── Program.cs                    # Swagger, DI: repos, UoW, PolicyFactory, AppService, ConciliationService
├── Conciliacao.Api.Tests/
│   ├── Fixtures/
│   │   ├── CustomWebApplicationFactory.cs
│   │   ├── FakeTransactionRepository.cs, FakeExternalEntryRepository.cs
│   ├── Infrastructure/               # TestConciliationDbContext, FailingTransactionRepository, etc.
│   ├── Integration/Idempotency/
│   │   └── ConciliationConcurrencyTests.cs
│   └── Reconciliation/
│       ├── ReconciliationControllerTests.cs
│       ├── ReconciliationTransactionTests.cs   # Um commit por batch
│       └── ReconciliationRollbackTests.cs      # Rollback em erro
├── Conciliacao.Application/
│   ├── DTOs/Reconciliation/           # BatchReconciliationRequestDto, ReconciliationBatchResponseDto, TransactionDto, etc.
│   ├── Requests/                      # ConciliationRequest, ConciliationItem
│   ├── Results/                       # ConciliationResult
│   ├── Factories/                     # IReconciliationPolicyFactory, ReconciliationPolicyFactory
│   ├── Mappers/                       # ReconciliationMapper
│   ├── Models/                        # ReconciliationBatchResult
│   └── Services/
│       ├── ReconciliationAppService.cs
│       ├── InternalBatchReconciliationService.cs
│       ├── ConciliationService.cs
│       └── IConciliationService.cs
├── Conciliacao.Domain/
│   ├── Entities/                     # Transaction, ExternalEntry, Client, ReconciliationItem, ProcessedRequest
│   ├── Repositories/                 # ITransactionRepository, IExternalEntryRepository, IProcessedRequestRepository, IUnitOfWork
│   ├── Policies/                     # IReconciliationPolicy, IReconciliationRule, Composite, Default, Rules, FakeRule
│   ├── Services/                     # SimpleReconciliationService
│   └── ValueObjects/                 # Money
├── Conciliacao.Infra/
│   ├── Contexts/                     # ConciliationDbContext, ConciliationDbContextFactory
│   ├── Repositories/                 # TransactionRepository, ExternalEntryRepository, ProcessedRequestRepository
│   ├── Persistence/                  # UnitOfWork (alternativa; Program usa DbContext como IUnitOfWork)
│   ├── Configurations/               # EF configurations
│   └── Migrations/
├── Conciliacao.Domain.Tests/
│   ├── SimpleReconciliationServiceTests.cs, ReconciliationAppServiceTests.cs, ReconciliationAppServiceFlowTests.cs
│   ├── DefaultReconciliationPolicyTests.cs, CompositeReconciliationPolicyTests.cs, MoneyTests.cs
│   ├── FakeReconciliationPolicyFactory.cs, FakeTransactionRepository.cs, FakeExternalEntryRepository.cs, FakeUnitOfWork.cs
│   └── Policies/Rules/               # ReferenceMatchRuleTests, etc.
└── docs/
    └── ARQUITETURA-E-FLUXO-CONCILIACAO.md   # Fluxo detalhado e diagramas
```

## 🚀 Quick Start

### Pré-requisitos

- .NET 10 SDK (ou a versão do projeto)
- Git
- SQL Server (ou LocalDB) para rodar a API com persistência real

### Instalação e execução

```bash
git clone https://github.com/awernek/Conciliacao.git
cd Conciliacao
dotnet build
dotnet test
```

Configure a connection string **DefaultConnection** em `appsettings.json` (ou `appsettings.Development.json`) para o ambiente desejado. Em ambiente **Testing**, a API usa fakes (não precisa de banco).

```bash
dotnet run --project Conciliacao.Api
```

Acesse o Swagger (ex.: `https://localhost:5xxx/swagger`).

### Exemplo: POST /api/reconciliation/batch

- **URL**: `POST /api/reconciliation/batch?clientCode=CLIENT_A`
- **Body**:
```json
{
  "transactions": [
    { "reference": "TX1", "amount": 100, "date": "2025-01-10" }
  ],
  "externalEntries": [
    { "reference": "TX1", "amount": 100, "date": "2025-01-10" }
  ]
}
```

### Exemplo: POST /api/conciliation (idempotente)

- **URL**: `POST /api/conciliation`
- **Header**: `Idempotency-Key: <GUID ou valor único>`
- **Body**:
```json
{
  "items": [
    { "reference": "REF-001", "amount": 100.50 },
    { "reference": "REF-002", "amount": 200.00 }
  ]
}
```

## 💡 Decisões de Design

| Decisão | Motivação |
|---------|-----------|
| **Dois fluxos de API** | Reconciliation batch: persistência + matching por cliente. Conciliation: idempotência e segurança em concorrência (ProcessedRequest + UNIQUE). |
| **Um commit por batch** | ReconciliationAppService persiste tudo, concilia e mapeia; CommitAsync() só no final. Falha antes = nada gravado. |
| **DbContext como IUnitOfWork** | Um único SaveChangesAsync por request; rollback implícito em exceção. |
| **Política por cliente (Factory)** | Regras/tolerâncias diferentes por cliente sem alterar o fluxo. |
| **InternalBatchReconciliationService** | Lógica de matching isolada; AppService orquestra persistência, factory e mapeamento. |
| **ProcessedRequest + UNIQUE** | Idempotência e tratamento de concorrência (apenas uma inserção por chave; demais retornam resultado já salvo). |

## 📚 Documentação Adicional

- [docs/ARQUITETURA-E-FLUXO-CONCILIACAO.md](docs/ARQUITETURA-E-FLUXO-CONCILIACAO.md) — Arquitetura, fluxo da conciliação em lote, diagramas (sequência, políticas, camadas).
- [docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md](docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md) — Idempotência, concorrência e consistência no banco.

## 🗺️ Roadmap

- [x] API REST: conciliação em lote (persistência + matching)
- [x] API REST: conciliação idempotente (Idempotency-Key)
- [x] Swagger/OpenAPI
- [x] Persistência com Entity Framework Core (SQL Server)
- [x] Unit of Work, repositórios, testes de transação e rollback
- [x] Testes de idempotência/concorrência
- [x] CI (GitHub Actions, master)
- [ ] Suporte multi-moeda
- [ ] Processamento assíncrono (mensageria)
- [ ] Dashboard e exportação de relatórios

## 📄 Licença

Este projeto está licenciado sob a Licença MIT — veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👤 Autor

**Anderson Wernek**  
- GitHub: [@awernek](https://github.com/awernek)

## 🤝 Contribuindo

Contribuições são bem-vindas. Abra uma issue ou envie um pull request.

1. Fork o projeto  
2. Crie sua branch (`git checkout -b feature/MinhaFeature`)  
3. Commit (`git commit -m 'Add: MinhaFeature'`)  
4. Push (`git push origin feature/MinhaFeature`)  
5. Abra um Pull Request  

---

⭐ Se este projeto foi útil, considere dar uma estrela no repositório.
