# Sistema de Conciliação Financeira

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Sistema de conciliação automatizada de transações financeiras, construído com Domain-Driven Design (DDD) e .NET 10.

##  Índice

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

##  Visão Geral

O Sistema de Conciliação Financeira automatiza a conciliação entre transações internas (ERP/Core) e lançamentos externos (bancos, gateways, APIs). Oferece **dois fluxos na mesma API** através de um único controller (`ConciliationController`): conciliação em lote (batch) e conciliação idempotente.

### Principais Funcionalidades

-  **POST /api/conciliation/batch**  Conciliação em lote: persiste transações e entradas externas, executa matching por política do cliente (Strategy + Composite), retorna Matched/Divergent/Missing/Extra e faz **um único commit** no final (rollback implícito em erro).
-  **POST /api/conciliation**  Conciliação **idempotente**: header `Idempotency-Key` obrigatório; requisições com a mesma chave retornam o resultado já salvo sem reprocessar (índice UNIQUE + tratamento de concorrência via `DuplicateKeyException`).
-  **Persistência**  Entity Framework Core, SQL Server; repositórios (Transaction, ExternalEntry, ProcessedRequest), Unit of Work (classe `UnitOfWork` dedicada, com tradução de exceções de infra para domínio).
-  **Política por cliente**  `IConciliationPolicyFactory` cria a política conforme `clientCode` (CLIENT_A, CLIENT_B, CLIENT_C) usando `CompositeReconciliationPolicy` com regras compostas.
-  **Clean Architecture**  Application não referencia EF Core nem SqlClient; dependência apenas do Domain. A tradução de `DbUpdateException`  `DuplicateKeyException` é feita no `UnitOfWork` (Infra).
-  **Testes**  Unitários (Domain, Application, Rules) e testes de API (WebApplicationFactory), incluindo transação (um commit por batch), rollback e idempotência/concorrência.
-  **CI**  GitHub Actions (build + test no branch `master`).

##  Arquitetura

### Visão de Contexto (C4 - Level 1)

```

                      SISTEMA DE CONCILIAÇÃO                             

                           
     Sistema                                        Bancos        
     Interno    ConciliationCtrl  ERPs          
    (ERP/Core)           (batch + idemp)            Gateways      
                           
                                                                     
                                                                     
    Transactions          ConciliationBatch            ExternalEntries  
    ExternalEntries       ResponseDto /                ProcessedRequests
                          ConciliationResult                            

```

### Camadas

```

  API (Controller)                                                       
  ConciliationController  POST /api/conciliation (idempotente)         
                          POST /api/conciliation/batch?clientCode=      
  Swagger / OpenAPI                                                      

  Application                                                            
  ConciliationBatchService   ConciliateBatchAsync (persiste + concilia)
  ConciliationService        ConciliateAsync (idempotente)             
  ConciliationPolicyFactory, ConciliationMapper, DTOs/Requests/Results  
   Não referencia EF Core  depende apenas do Domain                   

  Domain                                                                 
  Entities: Transaction, ExternalEntry, Client, ReconciliationItem,     
            ProcessedRequest                                             
  Exceptions: DuplicateKeyException                                      
  Repositories: ITransactionRepository, IExternalEntryRepository,       
                IProcessedRequestRepository, IUnitOfWork                 
  Policies: IReconciliationPolicy, CompositeReconciliationPolicy,       
            IReconciliationRule + Rules                                   
  Services: SimpleReconciliationService                                  
  ValueObjects: Money                                                    

  Infrastructure                                                         
  ConciliationDbContext, SqlServer                                       
  UnitOfWork (traduz DbUpdateException  DuplicateKeyException)         
  TransactionRepository, ExternalEntryRepository,                       
  ProcessedRequestRepository                                             
  Configurations, Migrations                                             

```

##  API REST

Ambos os endpoints vivem no **ConciliationController** (`/api/conciliation`):

### 1. Conciliação em lote (persistência + matching)

| Método | Rota | Descrição |
|--------|------|-----------|
| **POST** | `/api/conciliation/batch?clientCode={clientCode}` | Persiste transações e entradas externas, concilia com a política do cliente, retorna Matched/Divergent/Missing/Extra e faz **um commit** no final. |

- **clientCode** (query, obrigatório): define a política (ex.: CLIENT_A, CLIENT_B, CLIENT_C).
- **Body**: `ConciliationBatchRequestDto`  `Transactions`, `ExternalEntries`.
- **Response**: `ConciliationBatchResponseDto` (Matched, Divergent, Missing, Extra).
- Em erro: 500; o UoW não faz commit (rollback implícito).

### 2. Conciliação idempotente

| Método | Rota | Descrição |
|--------|------|-----------|
| **POST** | `/api/conciliation` | Processa a conciliação de forma **idempotente**. Header **Idempotency-Key** obrigatório. |

- **Header**: `Idempotency-Key` (obrigatório)  ex.: GUID ou valor único por operação.
- **Body**: `ConciliationRequest`  `Items` (Reference, Amount).
- **Response**: `ConciliationResult`  `Success`, `ProcessedCount`.
- Comportamento: primeira requisição com chave X  processa e persiste transações + ProcessedRequest; segunda requisição com a **mesma** chave X  não reprocessa, retorna o resultado já salvo. A concorrência é tratada pelo `UnitOfWork` que traduz a violação de UNIQUE para `DuplicateKeyException` (exceção de domínio).

##  Camada de Aplicação

| Componente | Descrição |
|------------|-----------|
| **ConciliationBatchService** | Recebe `Client` e listas de `TransactionDto`/`ExternalEntryDto`. Mapeia DTOEntity via `ConciliationMapper`, **persiste** (repositórios), obtém política via factory, executa **SimpleReconciliationService** (Domain), mapeia resultado para **ConciliationBatchResponseDto**, chama **CommitAsync()** e retorna. Um único commit por request; em exceção, nada é gravado. |
| **ConciliationService** | Fluxo idempotente: monta transações a partir de **ConciliationRequest**, persiste transações e **ProcessedRequest** (idempotencyKey + resultHash). Em violação de UNIQUE (concorrência), captura `DuplicateKeyException` e retorna **ConciliationResult** já salvo. |
| **IConciliationPolicyFactory** | Cria `IReconciliationPolicy` para um `Client` (CLIENT_A, CLIENT_B, CLIENT_C) usando `CompositeReconciliationPolicy` + regras. |
| **ConciliationMapper** | `ToEntity`/`ToDto` para Transaction e ExternalEntry. |

> **Nota**: a Application **não** referencia EF Core nem SqlClient. Toda exceção de infraestrutura é traduzida para `DuplicateKeyException` (Domain) pelo `UnitOfWork` (Infra).

##  Infraestrutura

- **ConciliationDbContext**  EF Core, SQL Server. DbSets: Transactions, ExternalEntries, ProcessedRequests.
- **UnitOfWork**  Implementa **IUnitOfWork** (`CommitAsync` = `SaveChangesAsync`). **Traduz** `DbUpdateException` (violação de chave única SQL Server, códigos 2601/2627) para `DuplicateKeyException` (exceção de domínio), isolando a camada Application de dependências de infraestrutura.
- **Repositórios**  TransactionRepository, ExternalEntryRepository, ProcessedRequestRepository (implementam interfaces do Domain).
- **Configurations**  Mapeamento EF para Transaction, ExternalEntry, ProcessedRequest, Conciliation.
- **Migrations**  Esquema inicial e índices (ex.: UNIQUE em ProcessedRequests.IdempotencyKey, ExternalEntry.Reference).
- Em ambiente **Testing** (ex.: testes de API), o Program.cs não registra os serviços de infra; os testes usam fakes (FakeTransactionRepository, FakeExternalEntryRepository, TestConciliationDbContext, etc.).

##  Políticas por Cliente

A **ConciliationPolicyFactory** utiliza **CompositeReconciliationPolicy** com regras compostas por código de cliente:

| ClientCode | Regras | Tolerância (valor) |
|------------|--------|---------------------|
| **CLIENT_A** | Reference + Date + Amount | 0,05 |
| **CLIENT_B** | Reference + Date + Amount | 0,00 (exata) |
| **CLIENT_C** | Reference + Amount (sem Date) | 0,10 |

Cada regra implementa `IReconciliationRule`. A `CompositeReconciliationPolicy` combina múltiplas regras usando o padrão Composite + Strategy.

##  Idempotência e Concorrência

- **Idempotência**: fluxo `POST /api/conciliation` com header `Idempotency-Key`. ProcessedRequest armazena chave + hash do resultado; requisições repetidas retornam o resultado já persistido (`FromPayload`).
- **Concorrência**: índice UNIQUE em `ProcessedRequests.IdempotencyKey`; duas requisições simultâneas com a mesma chave  uma insere, a outra recebe `DuplicateKeyException` (traduzida pelo `UnitOfWork` a partir de `DbUpdateException`), busca o ProcessedRequest pela chave e retorna o resultado já salvo.
- **Consistência no batch**: ConciliationBatchService persiste primeiro, concilia e mapeia; **só no final** chama CommitAsync(). Se algo falhar antes, nada é gravado. Controller com try/catch retorna 500 sem commit.

Detalhes: [docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md](docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md).

##  Estrutura do Projeto

```
Conciliacao/
 .github/workflows/
    dotnet.yml                    # CI: build + test (master)
 Conciliacao.Api/
    Controllers/
       ConciliationController.cs # POST /api/conciliation + /batch
    Program.cs                    # Swagger, DI: repos, UoW, Factory, Services
 Conciliacao.Api.Tests/
    Fixtures/
       CustomWebApplicationFactory.cs
       FakeTransactionRepository.cs
       FakeExternalEntryRepository.cs
    Infrastructure/               # TestConciliationDbContext, FailingTransactionRepository, etc.
    Integration/Idempotency/
        ConciliationConcurrencyTests.cs
 Conciliacao.Application/
    DTOs/Conciliation/            # ConciliationBatchRequestDto, ConciliationBatchResponseDto,
                                    # TransactionDto, ExternalEntryDto, MatchedPairDto
    Requests/                     # ConciliationRequest, ConciliationItem
    Results/                      # ConciliationResult
    Factories/                    # IConciliationPolicyFactory, ConciliationPolicyFactory
    Mappers/                      # ConciliationMapper
    Services/
        ConciliationBatchService.cs   # Conciliação em lote (fluxo batch)
        IConciliationBatchService.cs
        ConciliationService.cs        # Conciliação idempotente
        IConciliationService.cs
 Conciliacao.Domain/
    Entities/                     # Transaction, ExternalEntry, Client, ReconciliationItem,
                                    # ProcessedRequest, Conciliation
    Enums/                        # ReconciliationResult
    Exceptions/                   # DuplicateKeyException
    Policies/                     # IReconciliationPolicy, IReconciliationRule,
                                    # CompositeReconciliationPolicy,
                                    # ReferenceMatchRule, DateMatchRule, AmountToleranceRule
    Repositories/                 # ITransactionRepository, IExternalEntryRepository,
                                    # IProcessedRequestRepository, IUnitOfWork
    Services/                     # SimpleReconciliationService
    ValueObjects/                 # Money
 Conciliacao.Infra/
    Contexts/                     # ConciliationDbContext, ConciliationDbContextFactory
    Repositories/                 # TransactionRepository, ExternalEntryRepository,
                                    # ProcessedRequestRepository
    Persistence/                  # UnitOfWork (traduz DbUpdateException  DuplicateKeyException)
    Configurations/               # EF configurations
    Migrations/
 Conciliacao.Domain.Tests/
    SimpleReconciliationServiceTests.cs
    ConciliationBatchServiceTests.cs
    ConciliationBatchServiceFlowTests.cs
    DefaultReconciliationPolicyTests.cs
    MoneyTests.cs
    FakeConciliationPolicyFactory.cs
    FakeTransactionRepository.cs
    FakeExternalEntryRepository.cs
    FakeUnitOfWork.cs
    Policies/
        CompositeReconciliationPolicyTests.cs
        FakeRule.cs
        Rules/
            AmountToleranceRuleTests.cs
            DateMatchRuleTests.cs
            ReferenceMatchRuleTests.cs
 docs/
     COMO-FUNCIONA-O-PROJETO.md
     DIAGRAMAS-PROJETO.md
     ARQUITETURA-E-FLUXO-CONCILIACAO.md
     RECOMENDACOES-MELHORIA.md
     RECOMENDACOES-DDD-SOLID-CLEANCODE.md
     REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md
```

##  Quick Start

### Pré-requisitos

- .NET 10 SDK
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

### Exemplo: POST /api/conciliation/batch

- **URL**: `POST /api/conciliation/batch?clientCode=CLIENT_A`
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

##  Decisões de Design

| Decisão | Motivação |
|---------|-----------|
| **Controller único** | `ConciliationController` unifica ambos os fluxos (batch + idempotente) sob `/api/conciliation`, simplificando a API. |
| **Um commit por batch** | ConciliationBatchService persiste tudo, concilia e mapeia; CommitAsync() só no final. Falha antes = nada gravado. |
| **UnitOfWork dedicado** | Classe `UnitOfWork` (Infra) implementa `IUnitOfWork` e traduz exceções SQL  domínio (`DuplicateKeyException`), mantendo Application limpa. |
| **Política por cliente (Factory)** | `ConciliationPolicyFactory` + `CompositeReconciliationPolicy` permitem regras/tolerâncias diferentes por cliente sem alterar o fluxo. |
| **SimpleReconciliationService no Domain** | Lógica de matching pura no domínio; Application apenas orquestra persistência, factory e mapeamento. |
| **DuplicateKeyException (Domain)** | Exceção de domínio para violação de chave única. UnitOfWork (Infra) traduz DbUpdateException  DuplicateKeyException, eliminando dependência de EF Core na Application. |
| **ProcessedRequest + UNIQUE** | Idempotência e tratamento de concorrência (apenas uma inserção por chave; demais capturam `DuplicateKeyException` e retornam resultado já salvo). |
| **Client encapsulado** | Construtor com guarda de null; setter privado. Protege invariantes de domínio. |

##  Documentação Adicional

- [docs/COMO-FUNCIONA-O-PROJETO.md](docs/COMO-FUNCIONA-O-PROJETO.md)  Guia didático completo do sistema.
- [docs/DIAGRAMAS-PROJETO.md](docs/DIAGRAMAS-PROJETO.md)  12 diagramas Mermaid do sistema.
- [docs/ARQUITETURA-E-FLUXO-CONCILIACAO.md](docs/ARQUITETURA-E-FLUXO-CONCILIACAO.md)  Arquitetura, fluxo da conciliação, diagramas detalhados.
- [docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md](docs/REVISAO-IDEMPOTENCIA-CONCORRENCIA-CONSISTENCIA.md)  Idempotência, concorrência e consistência no banco.
- [docs/RECOMENDACOES-MELHORIA.md](docs/RECOMENDACOES-MELHORIA.md)  15 recomendações de melhoria (resolvidas).
- [docs/RECOMENDACOES-DDD-SOLID-CLEANCODE.md](docs/RECOMENDACOES-DDD-SOLID-CLEANCODE.md)  Revisão DDD, SOLID e Clean Code (resolvida).

##  Roadmap

- [x] API REST: conciliação em lote (persistência + matching)
- [x] API REST: conciliação idempotente (Idempotency-Key)
- [x] Swagger/OpenAPI
- [x] Persistência com Entity Framework Core (SQL Server)
- [x] Unit of Work dedicado com tradução de exceções
- [x] Testes de idempotência/concorrência
- [x] CI (GitHub Actions, master)
- [x] Clean Architecture  Application sem dependência de EF Core
- [x] DuplicateKeyException como exceção de domínio
- [ ] Suporte multi-moeda
- [ ] Processamento assíncrono (mensageria)
- [ ] Dashboard e exportação de relatórios

##  Licença

Este projeto está licenciado sob a Licença MIT  veja o arquivo LICENSE para detalhes.

##  Autor

**Anderson Wernek**
- GitHub: @awernek

##  Contribuindo

Contribuições são bem-vindas. Abra uma issue ou envie um pull request.

1. Fork o projeto
2. Crie sua branch (`git checkout -b feature/MinhaFeature`)
3. Commit (`git commit -m 'Add: MinhaFeature'`)
4. Push (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

---

 Se este projeto foi útil, considere dar uma estrela no repositório.