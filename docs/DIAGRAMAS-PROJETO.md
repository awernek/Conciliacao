# Diagramas do Sistema de Conciliação Financeira

> Todos os diagramas do projeto em Mermaid. Renderizam automaticamente no GitHub e GitLab.
> Também podem ser copiados para o [Mermaid Live Editor](https://mermaid.live).

---

## Diagrama 1 — Contexto Geral (System Design - Nivel 1)

Visão externa do sistema. Mostra quem interage com ele e de que forma.

```mermaid
graph TB
    subgraph Externos["Sistemas Externos"]
        Bank["🏦 Bancos<br/>(Extratos)"]
        Gateway["🔌 Gateways<br/>(Pagamentos)"]
        ERP["💼 ERP/Core<br/>(Transações)"]
    end

    subgraph Interno["Sistema de Conciliação<br/>(Sua aplicação)"]
        API["REST API"]
    end

    subgraph Usuários["Usuários"]
        Admin["👤 Admin"]
        System["🤖 Sistema<br/>de Retentativa"]
    end

    Bank -->|Envia extratos| API
    Gateway -->|Envia eventos| API
    ERP -->|Registra transações| API
    Admin -->|Acessa endpoints| API
    System -->|Processa em lote| API
    API -->|Retorna resultado<br/>Matched/Divergent/Missing/Extra| ERP
    API -->|Registra processamento| DB[(📊 Database<br/>SQL Server)]

    style Interno fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style Externos fill:#f3e5f5,stroke:#7b1fa2
    style Usuários fill:#e8f5e9,stroke:#388e3c
```

---

## Diagrama 2 — Fluxo de Dados de Alto Nível

Mostra como os dados fluem pela aplicação: entrada → processamento → saída.

```mermaid
graph LR
    subgraph Entrada["📥 Entrada"]
        TxInput["Transações<br/>(Sistema)"]
        ExtInput["Entradas Externas<br/>(Bancos)"]
    end

    subgraph Processamento["⚙️ Processamento"]
        Persist["Persistir"]
        Match["Matching<br/>(por política)"]
        Classify["Classificar<br/>Matched/Divergent<br/>Missing/Extra"]
    end

    subgraph Saida["📤 Saída"]
        Response["Response JSON"]
        Storage["Persistência<br/>no BD"]
    end

    TxInput -->|DTO| Persist
    ExtInput -->|DTO| Persist
    Persist -->|Entidades| Match
    Match -->|Resultado| Classify
    Classify -->|DTO| Response
    Classify -->|Entities| Storage

    style Entrada fill:#fff3e0
    style Processamento fill:#f3e5f5
    style Saida fill:#e8f5e9
```

---

## Diagrama 3 — Arquitetura de Containers (System Design - Nivel 2)

Mostra os principais "containers" (componentes deployáveis) do sistema.

```mermaid
graph TB
    User["👤 Usuário / Sistema<br/>Externo"]

    subgraph App["🖥️ Aplicação .NET"]
        API["REST API<br/>(Kestrel)"]
        AppLayer["Camada Application"]
        DomainLayer["Camada Domain"]
    end

    subgraph Data["💾 Persistência"]
        DB["SQL Server<br/>(Transações,<br/>ExternalEntries,<br/>ProcessedRequests)"]
    end

    User -->|HTTP<br/>POST /api/conciliation<br/>POST /api/conciliation/batch| API
    API -->|Usa| AppLayer
    AppLayer -->|Orquestra| DomainLayer
    AppLayer -->|Persiste| DB
    DomainLayer -->|Regras de negócio| AppLayer

    style App fill:#bbdefb,stroke:#1976d2,stroke-width:2px
    style Data fill:#c8e6c9,stroke:#388e3c,stroke-width:2px
```

---

## Diagrama 4 — Dois Fluxos Principais (High-Level)

Visão dos dois caminhos principais da API em paralelo.

```mermaid
graph TD
    subgraph Idempotent["🔐 Fluxo 1: Conciliação com idempotência"]
        I1["POST /api/conciliation"]
        I2["Header: Idempotency-Key"]
        I3["Garante 1 execução"]
        I4["Response: Success + Count"]
    end

    subgraph Batch["📦 Fluxo 2: Conciliação em lote (sem idempotência)"]
        B1["POST /api/conciliation/batch"]
        B2["clientCode: CLIENT_A/B/C"]
        B3["Persiste + Concilia + Commit"]
        B4["Response: Matched/Divergent<br/>Missing/Extra"]
    end

    subgraph Database["💾 Resultado"]
        DB["Dados persistidos<br/>atomicamente"]
    end

    I1 --> I2 --> I3 --> I4 --> DB
    B1 --> B2 --> B3 --> B4 --> DB

    style Batch fill:#fff9c4,stroke:#f57f17,stroke-width:2px
    style Idempotent fill:#c8e6c9,stroke:#388e3c,stroke-width:2px
    style Database fill:#bbdefb,stroke:#1976d2,stroke-width:2px
```

---

## Diagrama 5 — Visão Geral das Camadas

Mostra como o projeto está organizado em 4 camadas (Clean Architecture).

```mermaid
flowchart TB
    subgraph API["🌐 API (Conciliacao.Api)"]
        direction LR
        CC["ConciliationController<br/>POST /api/conciliation e /api/conciliation/batch"]
    end

    subgraph APP["Application (Conciliacao.Application)"]
        direction LR
        CBS["ConciliationBatchService"]
        CS["ConciliationService"]
        SRS["SimpleReconciliation<br/>Service (Domain)"]
        FAC["ConciliationPolicyFactory"]
        MAP["ConciliationMapper"]
    end

    subgraph DOM["Domain (Conciliacao.Domain)"]
        direction LR
        ENT["Entidades"]
        POL["Policies"]
        RUL["Rules"]
        MON["Money"]
        SRS["SimpleReconciliation<br/>Service"]
    end

    subgraph INFRA["Infrastructure (Conciliacao.Infra)"]
        direction LR
        CTX["DbContext"]
        TXR["TransactionRepository"]
        EXR["ExternalEntryRepository"]
        PRR["ProcessedRequestRepository"]
        DB[("SQL Server")]
    end

    CC --> CBS
    CC --> CS
    CBS --> MAP
    CBS --> FAC
    CBS --> SRS
    CBS --> TXR
    CBS --> EXR
    CS --> TXR
    CS --> PRR
    FAC --> POL
    SRS --> POL
    POL --> RUL
    RUL --> MON
    CTX --> DB
    TXR --> CTX
    EXR --> CTX
    PRR --> CTX
```

---

## Diagrama 6 — Fluxo Batch (Da requisição à resposta)

Mostra passo a passo o que acontece quando alguém chama `POST /api/conciliation/batch?clientCode=CLIENT_A`.

```mermaid
sequenceDiagram
    autonumber
    actor U as Usuario
    participant C as ConciliationController
    participant A as ConciliationBatchService
    participant M as ConciliationMapper
    participant R as Repositorios
    participant F as ConciliationPolicyFactory
    participant B as SimpleReconciliationService
    participant P as IReconciliationPolicy
    participant UW as UnitOfWork

    U->>C: POST /api/conciliation/batch?clientCode=CLIENT_A
    Note right of U: Body: transactions + externalEntries

    C->>C: Cria objeto Client com clientCode
    C->>A: ConciliateBatchAsync(client, transactionDTOs, externalEntryDTOs)

    rect rgb(230, 245, 255)
        Note over A,M: Etapa 1 - Mapeamento
        A->>M: ToEntity(transactionDTOs)
        M-->>A: Lista de Transaction (entidades)
        A->>M: ToEntity(externalEntryDTOs)
        M-->>A: Lista de ExternalEntry (entidades)
    end

    rect rgb(230, 255, 230)
        Note over A,R: Etapa 2 - Persistencia
        A->>R: AddRangeAsync(transactions)
        A->>R: AddRangeAsync(externalEntries)
        Note right of R: Dados ficam em memoria (sem commit ainda!)
    end

    rect rgb(255, 245, 230)
        Note over A,P: Etapa 3 - Conciliacao
        A->>F: CreateFor(client)
        F-->>A: CompositeReconciliationPolicy com regras do cliente
        A->>B: Reconcile(transactions, externalEntries)

        loop Para cada Transaction
            B->>B: Busca ExternalEntry com mesma Reference
            alt Nao encontrou referencia
                B->>B: Classifica como MISSING
            else Encontrou referencia
                B->>P: IsMatch(transaction, externalEntry)
                alt Todas as regras satisfeitas
                    P-->>B: true - MATCHED
                else Alguma regra falhou
                    P-->>B: false - DIVERGENT
                end
            end
        end
        B->>B: ExternalEntries sem par = EXTRA
        B-->>A: ReconciliationItem[] (Matched, Divergent, Missing, Extra)
    end

    rect rgb(255, 230, 255)
        Note over A,UW: Etapa 4 - Commit
        A->>A: Mapeia Result para ResponseDTO
        A->>UW: CommitAsync()
        Note right of UW: Agora sim grava tudo no banco!
    end

    A-->>C: ConciliationBatchResponseDto
    C-->>U: 200 OK + JSON com Matched, Divergent, Missing, Extra
```

---

## Diagrama 7 — Fluxo Idempotente

Mostra como funciona o `POST /api/conciliation` com `Idempotency-Key`.

```mermaid
sequenceDiagram
    autonumber
    actor U as Usuario
    participant C as ConciliationController
    participant S as ConciliationService
    participant TR as TransactionRepository
    participant PR as ProcessedRequestRepository
    participant UW as UnitOfWork

    U->>C: POST /api/conciliation
    Note right of U: Header: Idempotency-Key = abc-123<br/>Body: items com reference e amount

    alt Header Idempotency-Key ausente
        C-->>U: 400 Bad Request
    end

    C->>S: ConciliateAsync(request, "abc-123")

    S->>S: Converte items em Transaction entities
    S->>S: Cria ConciliationResult(success, count)
    S->>TR: AddRangeAsync(transactions)
    S->>PR: AddAsync(ProcessedRequest com key + payload)
    S->>UW: CommitAsync()

    alt Commit com sucesso (primeira vez)
        UW-->>S: OK
        S-->>C: ConciliationResult (success=true)
        C-->>U: 200 OK
    else DbUpdateException - chave duplicada
        Note over S,PR: Outra requisicao ja salvou com mesma key!
        S->>PR: GetByKeyAsync("abc-123")
        PR-->>S: ProcessedRequest ja salvo
        S->>S: FromPayload(resultHash) - reconstroi resultado
        S-->>C: ConciliationResult do registro anterior
        C-->>U: 200 OK (mesmo resultado!)
    end
```

---

## Diagrama 8 — Políticas de Conciliação (Strategy + Composite)

Mostra como as regras de matching são organizadas.

```mermaid
classDiagram
    direction TB

    class IReconciliationPolicy {
        <<interface>>
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class CompositeReconciliationPolicy {
        -rules : IEnumerable~IReconciliationRule~
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class DefaultReconciliationPolicy {
        -tolerance : decimal
        +IsMatch(Transaction, ExternalEntry) bool
    }

    class IReconciliationRule {
        <<interface>>
        +IsSatisfied(Transaction, ExternalEntry) bool
    }

    class ReferenceMatchRule {
        +IsSatisfied(Transaction, ExternalEntry) bool
    }

    class DateMatchRule {
        +IsSatisfied(Transaction, ExternalEntry) bool
    }

    class AmountToleranceRule {
        -tolerance : decimal
        +IsSatisfied(Transaction, ExternalEntry) bool
    }

    class Money {
        <<ValueObject>>
        +Amount : decimal
        +Equals(Money other, decimal tolerance) bool
    }

    class ConciliationPolicyFactory {
        +CreateFor(Client) IReconciliationPolicy
    }

    class Client {
        +Code : string
    }

    IReconciliationPolicy <|.. CompositeReconciliationPolicy : implementa
    IReconciliationPolicy <|.. DefaultReconciliationPolicy : implementa
    CompositeReconciliationPolicy o-- IReconciliationRule : contem varias regras
    IReconciliationRule <|.. ReferenceMatchRule : implementa
    IReconciliationRule <|.. DateMatchRule : implementa
    IReconciliationRule <|.. AmountToleranceRule : implementa
    AmountToleranceRule --> Money : usa
    ConciliationPolicyFactory --> Client : recebe
    ConciliationPolicyFactory --> CompositeReconciliationPolicy : cria
```

---

## Diagrama 9 — Configuração por Cliente

Mostra quais regras cada cliente usa.

```mermaid
flowchart LR
    F["PolicyFactory<br/>CreateFor(client)"]

    F -->|CLIENT_A| A["CompositePolicy"]
    A --> A1["ReferenceMatchRule"]
    A --> A2["DateMatchRule"]
    A --> A3["AmountToleranceRule<br/>tolerancia: 0.05"]

    F -->|CLIENT_B| B["CompositePolicy"]
    B --> B1["ReferenceMatchRule"]
    B --> B2["DateMatchRule"]
    B --> B3["AmountToleranceRule<br/>tolerancia: 0.00 (exata)"]

    F -->|CLIENT_C| CL["CompositePolicy"]
    CL --> C1["ReferenceMatchRule"]
    CL --> C3["AmountToleranceRule<br/>tolerancia: 0.10"]

    style A fill:#d4edda
    style B fill:#cce5ff
    style CL fill:#fff3cd
```

---

## Diagrama 10 — Entidades do Domínio

Mostra as principais entidades e seus atributos.

```mermaid
classDiagram
    direction LR

    class Transaction {
        +Id : Guid
        +Amount : decimal
        +Date : DateTime
        +Reference : string
        +ExternalReference : string
    }

    class ExternalEntry {
        +Id : int
        +Amount : decimal
        +Date : DateTime
        +Reference : string
        +Source : string
    }

    class Client {
        +Code : string
    }

    class ProcessedRequest {
        +Id : Guid
        +IdempotencyKey : string
        +ResultHash : string
        +ProcessedAt : DateTime
    }

    class ReconciliationItem {
        +Transaction : Transaction
        +ExternalEntry : ExternalEntry
        +Result : ReconciliationResult
    }

    class ReconciliationResult {
        <<enumeration>>
        Matched
        Divergent
        Missing
        Extra
    }

    ReconciliationItem --> Transaction
    ReconciliationItem --> ExternalEntry
    ReconciliationItem --> ReconciliationResult
```

---

## Diagrama 11 — Consistência: Unit of Work

Mostra como o commit único garante atomicidade.

```mermaid
flowchart TD
    REQ["Requisicao chega"]
    MAP["Mapeia DTOs para Entidades"]
    PERSIST["Persiste em memoria<br/>(AddRange nos repositorios)"]
    CONCILIA["Executa conciliacao<br/>(matching por politica)"]
    MONTA["Monta response DTO"]

    COMMIT{"CommitAsync()"}
    OK["200 OK + Resposta JSON<br/>Dados gravados no banco"]
    ERRO["Excecao antes do commit"]
    NADA["Nada foi gravado!<br/>500 Internal Server Error"]

    REQ --> MAP --> PERSIST --> CONCILIA --> MONTA --> COMMIT
    COMMIT -->|Sucesso| OK
    COMMIT -->|Erro| NADA

    PERSIST -.->|Qualquer erro aqui| ERRO
    CONCILIA -.->|Qualquer erro aqui| ERRO
    MONTA -.->|Qualquer erro aqui| ERRO
    ERRO --> NADA

    style OK fill:#d4edda,stroke:#28a745
    style NADA fill:#f8d7da,stroke:#dc3545
```

---

## Diagrama 12 — Concorrência na Idempotência

Mostra o que acontece quando duas requisições idênticas chegam ao mesmo tempo.

```mermaid
sequenceDiagram
    autonumber
    actor R1 as Requisicao 1
    actor R2 as Requisicao 2
    participant S as ConciliationService
    participant DB as Banco de Dados

    Note over R1,R2: Duas requisicoes simultaneas<br/>com mesma Idempotency-Key

    par Processamento paralelo
        R1->>S: ConciliateAsync(request, "KEY-X")
        S->>S: Processa e monta resultado
        S->>DB: INSERT ProcessedRequest (KEY-X)
    and
        R2->>S: ConciliateAsync(request, "KEY-X")
        S->>S: Processa e monta resultado
        S->>DB: INSERT ProcessedRequest (KEY-X)
    end

    Note over DB: Indice UNIQUE em IdempotencyKey!<br/>Apenas UM insert funciona

    DB-->>S: Requisicao 1: OK (inseriu primeiro)
    S-->>R1: ConciliationResult original

    DB-->>S: Requisicao 2: DbUpdateException!
    S->>DB: SELECT ProcessedRequest WHERE Key = KEY-X
    DB-->>S: Registro ja salvo pela Req 1
    S->>S: FromPayload() - reconstroi resultado
    S-->>R2: Mesmo ConciliationResult!

    Note over R1,R2: Ambas recebem o MESMO resultado<br/>Nenhuma duplicata foi criada
```
