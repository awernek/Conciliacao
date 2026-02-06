# Diagramas do Sistema de Conciliação Financeira

> Todos os diagramas do projeto em Mermaid. Renderizam automaticamente no GitHub e GitLab.

---

##  Diagramas de Alto Nível (Vision / System Design)

### 1. Contexto Geral (System Design - Nível 1)

```mermaid
graph LR
    subgraph Fontes Externas
        B[" Bancos"]
        G[" Gateways"]
        E[" ERPs"]
    end

    subgraph Sistema Interno
        SI[" Sistema Interno<br/>(ERP/Core)"]
    end

    subgraph Sistema de Conciliação
        API[" ConciliationController<br/>POST /api/conciliation<br/>POST /api/conciliation/batch"]
        APP[" Application<br/>ConciliationBatchService<br/>ConciliationService"]
        DOM[" Domain<br/>SimpleConciliationService<br/>Policies + Rules"]
        DB[" SQL Server"]
    end

    SI -->|Transactions| API
    B -->|ExternalEntries| API
    G -->|ExternalEntries| API
    E -->|ExternalEntries| API
    API --> APP
    APP --> DOM
    APP --> DB
```

---

### 2. Fluxo de Dados de Alto Nível

```mermaid
flowchart LR
    subgraph Input
        TX[" Transactions<br/>(sistema interno)"]
        EX[" ExternalEntries<br/>(banco/gateway)"]
    end

    subgraph Processamento
        P["1 Persistir<br/>(Repositories)"]
        M["2 Matching<br/>(SimpleConciliationService<br/>+ Policy)"]
        C["3 Classificar<br/>(Matched/Divergent/<br/>Missing/Extra)"]
    end

    subgraph Output
        R[" ConciliationBatchResponseDto<br/>(JSON)"]
    end

    TX --> P
    EX --> P
    P --> M
    M --> C
    C --> R
```

---

### 3. Arquitetura de Containers (System Design - Nível 2)

```mermaid
graph TB
    subgraph ".NET App (Conciliacao)"
        API[" Conciliacao.Api<br/>ASP.NET Core<br/>ConciliationController"]
        APP[" Conciliacao.Application<br/>ConciliationBatchService<br/>ConciliationService<br/>Mapper / Factory"]
        DOM[" Conciliacao.Domain<br/>Entities / Policies / Services<br/>Exceptions / ValueObjects"]
        INFRA[" Conciliacao.Infra<br/>EF Core / UnitOfWork<br/>Repositories"]
    end

    DB[" SQL Server<br/>Transactions, ExternalEntries,<br/>ProcessedRequests"]

    API --> APP
    APP --> DOM
    INFRA --> DOM
    INFRA --> DB
```

---

### 4. Dois Fluxos Principais (High-Level)

```mermaid
flowchart LR
    subgraph "Fluxo 1: Batch"
        B1["POST /api/conciliation/batch<br/>?clientCode=CLIENT_A"]
        B2["ConciliationBatchService"]
        B3["SimpleConciliationService<br/>+ CompositePolicy"]
        B4["ConciliationBatchResponseDto<br/>Matched/Divergent/Missing/Extra"]
        B1 --> B2 --> B3 --> B4
    end

    subgraph "Fluxo 2: Idempotente"
        I1["POST /api/conciliation<br/>Idempotency-Key: KEY-X"]
        I2["ConciliationService"]
        I3["ProcessedRequest<br/>+ UNIQUE + DuplicateKeyException"]
        I4["ConciliationResult<br/>Success + ProcessedCount"]
        I1 --> I2 --> I3 --> I4
    end
```

---

##  Diagramas Técnicos Detalhados

### 5. Visão Geral das Camadas

```mermaid
graph TB
    subgraph " API"
        CC["ConciliationController<br/>POST /conciliation<br/>POST /conciliation/batch"]
    end

    subgraph " Application"
        CBS["ConciliationBatchService"]
        CS["ConciliationService"]
        CPF["ConciliationPolicyFactory"]
        CM["ConciliationMapper"]
    end

    subgraph " Domain"
        SRS["SimpleConciliationService"]
        POL["CompositeConciliationPolicy"]
        RULES["ReferenceMatchRule<br/>DateMatchRule<br/>AmountToleranceRule"]
        ENT["Transaction | ExternalEntry<br/>Client | ConciliationItem<br/>ProcessedRequest"]
        DKE["DuplicateKeyException"]
        MO["Money (Value Object)"]
        REPO_I["ITransactionRepository<br/>IExternalEntryRepository<br/>IProcessedRequestRepository<br/>IUnitOfWork"]
    end

    subgraph " Infrastructure"
        CTX["ConciliationDbContext"]
        UOW["UnitOfWork<br/>(traduz exceções)"]
        REPO["TransactionRepository<br/>ExternalEntryRepository<br/>ProcessedRequestRepository"]
    end

    CC --> CBS
    CC --> CS
    CBS --> CM
    CBS --> CPF
    CBS --> SRS
    CBS --> REPO_I
    CS --> REPO_I
    CS -.-> DKE
    CPF --> POL
    POL --> RULES
    RULES --> MO
    SRS --> POL
    SRS --> ENT
    REPO --> REPO_I
    UOW --> REPO_I
    UOW -.-> DKE
    CTX --> REPO
```

---

### 6. Fluxo Batch (Da requisição à resposta)

```mermaid
sequenceDiagram
    participant U as Usuário
    participant C as ConciliationController
    participant S as ConciliationBatchService
    participant M as ConciliationMapper
    participant TR as TransactionRepository
    participant ER as ExternalEntryRepository
    participant F as ConciliationPolicyFactory
    participant RS as SimpleConciliationService
    participant UoW as UnitOfWork

    U->>C: POST /api/conciliation/batch?clientCode=CLIENT_A
    C->>C: new Client(clientCode)
    C->>S: ConciliateBatchAsync(client, txDtos, entryDtos)

    rect rgb(230, 240, 255)
        Note over S,M: 1. Mapeamento DTO  Entity
        S->>M: ToEntity(transactionDtos)
        S->>M: ToEntity(externalEntryDtos)
    end

    rect rgb(230, 255, 230)
        Note over S,ER: 2. Persistência
        S->>TR: AddRangeAsync(transactions)
        S->>ER: AddRangeAsync(externalEntries)
    end

    rect rgb(255, 245, 230)
        Note over S,RS: 3. Matching
        S->>F: CreateFor(client)
        F-->>S: CompositePolicy(rules)
        S->>RS: new SimpleConciliationService(policy)
        S->>RS: Conciliate(transactions, externalEntries)
        RS-->>S: List<ConciliationItem>
    end

    rect rgb(240, 230, 255)
        Note over S,M: 4. Mapeamento Entity  DTO
        S->>M: ToDto(Matched/Divergent/Missing/Extra)
    end

    rect rgb(255, 230, 230)
        Note over S,UoW: 5. Commit atômico
        S->>UoW: CommitAsync()
    end

    S-->>C: ConciliationBatchResponseDto
    C-->>U: 200 OK + JSON
```

---

### 7. Fluxo Idempotente

```mermaid
sequenceDiagram
    participant U as Usuário
    participant C as ConciliationController
    participant S as ConciliationService
    participant TR as TransactionRepository
    participant PR as ProcessedRequestRepository
    participant UoW as UnitOfWork
    participant DB as SQL Server

    U->>C: POST /api/conciliation<br/>Idempotency-Key: KEY-X
    C->>S: ConciliateAsync(request, "KEY-X")

    alt Primeira requisição com KEY-X
        S->>S: Cria transações a partir dos items
        S->>TR: AddRangeAsync(transactions)
        S->>PR: AddAsync(ProcessedRequest{KEY-X, hash})
        S->>UoW: CommitAsync()
        UoW->>DB: SaveChangesAsync
        DB-->>UoW:  OK
        S-->>C: ConciliationResult(success, count)
    else KEY-X já existe (duplicata ou concorrência)
        S->>UoW: CommitAsync()
        UoW->>DB: SaveChangesAsync
        DB-->>UoW:  UNIQUE violation
        UoW-->>S: DuplicateKeyException
        S->>PR: GetByKeyAsync("KEY-X")
        PR-->>S: ProcessedRequest
        S-->>C: ConciliationResult.FromPayload(hash)
    end

    C-->>U: 200 OK + ConciliationResult
```

---

### 8. Políticas de Conciliação (Strategy + Composite)

```mermaid
classDiagram
    class IConciliationPolicy {
        <<interface>>
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class CompositeConciliationPolicy {
        -rules: IConciliationRule[]
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class IConciliationRule {
        <<interface>>
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class ReferenceMatchRule {
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class DateMatchRule {
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class AmountToleranceRule {
        -tolerance: decimal
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class Money {
        +Amount: decimal
        +IsWithinTolerance(Money, decimal) bool
        +Equals(Money) bool
    }

    class ConciliationPolicyFactory {
        +CreateFor(Client) IConciliationPolicy
    }

    IConciliationPolicy <|.. CompositeConciliationPolicy
    IConciliationRule <|.. ReferenceMatchRule
    IConciliationRule <|.. DateMatchRule
    IConciliationRule <|.. AmountToleranceRule
    CompositeConciliationPolicy o-- IConciliationRule : combina 1..*
    AmountToleranceRule --> Money : compara via
    ConciliationPolicyFactory ..> CompositeConciliationPolicy : cria
    ConciliationPolicyFactory ..> IConciliationRule : seleciona
```

---

### 9. Configuração por Cliente

```mermaid
flowchart TD
    F["ConciliationPolicyFactory.CreateFor(client)"]
    F --> A{client.Code?}

    A -->|CLIENT_A| PA["CompositePolicy"]
    PA --> PA1[" ReferenceMatchRule"]
    PA --> PA2[" DateMatchRule"]
    PA --> PA3[" AmountToleranceRule(0.05)"]

    A -->|CLIENT_B| PB["CompositePolicy"]
    PB --> PB1[" ReferenceMatchRule"]
    PB --> PB2[" DateMatchRule"]
    PB --> PB3[" AmountToleranceRule(0.00)"]

    A -->|CLIENT_C| PC["CompositePolicy"]
    PC --> PC1[" ReferenceMatchRule"]
    PC --> PC3[" AmountToleranceRule(0.10)"]

    A -->|Outro| ERR[" ArgumentException"]

    style PA fill:#d4edda
    style PB fill:#cce5ff
    style PC fill:#fff3cd
    style ERR fill:#f8d7da
```

---

### 10. Entidades do Domínio

```mermaid
classDiagram
    class Transaction {
        +Id: int
        +ExternalReference: string
        +Reference: string
        +Amount: decimal
        +Date: DateTime
    }

    class ExternalEntry {
        +Id: int
        +Reference: string
        +Amount: decimal
        +Date: DateTime
    }

    class Client {
        +Code: string
        +Client(code: string)
    }

    class ProcessedRequest {
        +Id: int
        +IdempotencyKey: string
        +ResultHash: string
        +ProcessedAt: DateTime
        +ProcessedRequest(key, hash)
    }

    class ConciliationItem {
        +Transaction: Transaction?
        +ExternalEntry: ExternalEntry?
        +Status: ConciliationStatus
    }

    class ConciliationStatus {
        <<enumeration>>
        Matched
        Divergent
        Missing
        Extra
    }

    class DuplicateKeyException {
        +DuplicateKeyException()
        +DuplicateKeyException(message, inner)
    }

    ConciliationItem --> Transaction
    ConciliationItem --> ExternalEntry
    ConciliationItem --> ConciliationStatus
```

---

### 11. Consistência: Unit of Work

```mermaid
flowchart TD
    START["ConciliationBatchService.ConciliateBatchAsync()"]
    START --> ADD["AddRangeAsync(transactions)<br/>AddRangeAsync(externalEntries)"]
    ADD --> MATCH["SimpleConciliationService.Conciliate()"]
    MATCH --> MAP["ConciliationMapper.ToDto()"]
    MAP --> COMMIT{"UnitOfWork.CommitAsync()"}

    COMMIT -->|Sucesso| OK[" Tudo salvo no banco<br/>(um único SaveChangesAsync)"]
    COMMIT -->|Exceção antes do commit| ROLLBACK[" Nada salvo<br/>(rollback implícito)"]
    COMMIT -->|UNIQUE violation| DKE["DuplicateKeyException<br/>(traduzida pelo UnitOfWork)"]

    style OK fill:#d4edda
    style ROLLBACK fill:#f8d7da
    style DKE fill:#fff3cd
```

---

### 12. Concorrência na Idempotência

```mermaid
sequenceDiagram
    participant R1 as Requisição 1
    participant R2 as Requisição 2
    participant S as ConciliationService
    participant UoW as UnitOfWork
    participant DB as SQL Server (UNIQUE)

    Note over R1,R2: Duas requisições simultâneas com mesma Idempotency-Key

    R1->>S: ConciliateAsync(req, "KEY-X")
    R2->>S: ConciliateAsync(req, "KEY-X")

    S->>S: [R1] Cria transactions + ProcessedRequest
    S->>S: [R2] Cria transactions + ProcessedRequest

    S->>UoW: [R1] CommitAsync()
    UoW->>DB: [R1] SaveChangesAsync
    DB-->>UoW: [R1]  Inserido

    S->>UoW: [R2] CommitAsync()
    UoW->>DB: [R2] SaveChangesAsync
    DB-->>UoW: [R2]  UNIQUE violation (2601/2627)
    UoW-->>S: [R2] DuplicateKeyException

    S->>DB: [R2] GetByKeyAsync("KEY-X")
    DB-->>S: [R2] ProcessedRequest (já salvo por R1)

    S-->>R1: ConciliationResult (processado)
    S-->>R2: ConciliationResult (recuperado do banco)

    Note over R1,R2: Ambas retornam o mesmo resultado 
```

---

> **Nota:** Todos os diagramas usam nomes que correspondem exatamente às classes no código-fonte atual. `CompositeConciliationPolicy` é a única implementação de política (`IConciliationPolicy`). A tradução de exceções SQL  `DuplicateKeyException` é feita pelo `UnitOfWork` na infraestrutura.