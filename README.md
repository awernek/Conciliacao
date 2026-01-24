# Sistema de Conciliação Financeira

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Sistema de reconciliação automatizada de transações financeiras, construído com Domain-Driven Design (DDD) e .NET 10.

## 📋 Índice

- [Visão Geral](#-visão-geral)
- [Arquitetura](#-arquitetura)
- [Modelo de Domínio](#-modelo-de-domínio)
- [Fluxo de Reconciliação](#-fluxo-de-reconciliação)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Quick Start](#-quick-start)
- [Decisões de Design](#-decisões-de-design)
- [Extensibilidade](#-extensibilidade)
- [Roadmap](#-roadmap)
- [Licença](#-licença)

## 🎯 Visão Geral

O Sistema de Conciliação Financeira automatiza o processo de reconciliação entre transações internas (ERP/Core) e lançamentos externos (bancos, gateways de pagamento, APIs). Utilizando estratégias configuráveis de matching, o sistema identifica transações correspondentes, detecta divergências e destaca lançamentos extras ou faltantes.

### Principais Funcionalidades

- ✅ Matching automático de transações com base em regras configuráveis
- ✅ Comparação monetária com tolerância para diferenças de arredondamento
- ✅ Identificação de transações matched, missing e extra
- ✅ Arquitetura extensível baseada em Strategy Pattern
- ✅ Testes unitários com cobertura completa

## 🏗️ Arquitetura

### Visão de Contexto (C4 - Level 1)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                      SISTEMA DE CONCILIAÇÃO                             │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│    ┌──────────┐         ┌──────────────────┐         ┌───────────┐    │
│    │ Sistema  │         │                  │         │  Bancos   │    │
│    │ Interno  │────────▶│  Reconciliation  │◀────────│  ERPs     │    │
│    │(ERP/Core)│         │     Engine       │         │  Gateways │    │
│    └──────────┘         │                  │         └───────────┘    │
│          │              └──────────────────┘               │           │
│          │                       │                         │           │
│          │                       │                         │           │
│    ┌──────────┐         ┌──────────────────┐     ┌──────────────┐    │
│    │Transactions│        │ReconciliationItems│    │ExternalEntries│   │
│    └──────────┘         │  ┌──────────────┐│     └──────────────┘    │
│                         │  │Match│Miss│Extra││                        │
│                         │  └──────────────┘│                         │
│                         └──────────────────┘                         │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

### Arquitetura em Camadas

```
┌───────────────────────────────────────────────────────┐
│              API Layer (ASP.NET Core)                 │
│                 [Em desenvolvimento]                  │
├───────────────────────────────────────────────────────┤
│              Application Layer                        │
│        Orquestração e Casos de Uso                    │
├───────────────────────────────────────────────────────┤
│                 Domain Layer                          │
│  ┌─────────┐  ┌─────────┐  ┌──────────────────┐     │
│  │Entities │  │  Value  │  │    Services      │     │
│  │         │  │ Objects │  │                  │     │
│  │Transaction  │  Money  │  │SimpleReconciliation   │
│  │ExternalEntry│         │  │     Service      │     │
│  │Reconciliation│        │  │                  │     │
│  │   Item   │  │         │  │                  │     │
│  └─────────┘  └─────────┘  └──────────────────┘     │
│                                  │                    │
│  ┌───────────────────────────────────────────────┐   │
│  │              Policies                         │   │
│  │  ┌──────────────────┐  ┌──────────────────┐  │   │
│  │  │IReconciliationPolicy││DefaultReconciliation│ │   │
│  │  │   (interface)    │  │     Policy       │  │   │
│  │  └──────────────────┘  └──────────────────┘  │   │
│  └───────────────────────────────────────────────┘   │
├───────────────────────────────────────────────────────┤
│            Infrastructure Layer                       │
│              [Em desenvolvimento]                     │
│        Database Access, External APIs, etc.           │
└───────────────────────────────────────────────────────┘
```

## 📊 Modelo de Domínio

```mermaid
classDiagram
    class Transaction {
        +int Id
        +decimal Amount
        +DateTime Date
        +string Reference
    }
    
    class ExternalEntry {
        +int Id
        +decimal Amount
        +DateTime Date
        +string Reference
        +string Source
    }
    
    class Money {
        +decimal Amount
        +Equals(Money, tolerance) bool
    }
    
    class ReconciliationItem {
        +Transaction? Transaction
        +ExternalEntry? ExternalEntry
        +ReconciliationResult Result
    }
    
    class ReconciliationResult {
        <<enumeration>>
        Matched
        Missing
        Extra
        Divergent
    }
    
    class IReconciliationPolicy {
        <<interface>>
        +IsMatch(Transaction, ExternalEntry) bool
    }
    
    class DefaultReconciliationPolicy {
        -decimal tolerance
    }
    
    class SimpleReconciliationService {
        +Reconcile(transactions, entries) ReconciliationItem[]
    }
    
    SimpleReconciliationService --> IReconciliationPolicy : usa
    DefaultReconciliationPolicy ..|> IReconciliationPolicy
    DefaultReconciliationPolicy --> Money : compara valores
    ReconciliationItem --> ReconciliationResult
```

## 🔄 Fluxo de Reconciliação

### Visão Geral do Processo

```
       ENTRADA                    PROCESSAMENTO                    SAÍDA
┌─────────────────┐                                        ┌─────────────────┐
│  Transactions   │──┐                                   ┌─▶│    Matched      │
│ (Sistema Int.)  │  │   ┌─────────────────────────┐    │  │  tx ↔ ext OK    │
└─────────────────┘  │   │                         │    │  └─────────────────┘
                     ├──▶│SimpleReconciliation     │────┤
┌─────────────────┐  │   │       Service           │    │  ┌─────────────────┐
│ExternalEntries  │──┘   │                         │    ├─▶│    Missing      │
│(Bancos, APIs)   │      │ ┌─────────────────────┐ │    │  │  tx sem ext     │
└─────────────────┘      │ │IReconciliationPolicy│ │    │  └─────────────────┘
                         │ │                     │ │    │
                         │ │ • Reference match   │ │    │  ┌─────────────────┐
                         │ │ • Date match        │ │    └─▶│     Extra       │
                         │ │ • Amount ± tolerance│ │       │  ext sem tx     │
                         │ └─────────────────────┘ │       └─────────────────┘
                         └─────────────────────────┘
```

### Algoritmo de Matching

```
PARA CADA transaction em Transactions:
    match ← Buscar ExternalEntry onde Policy.IsMatch(tx, ext) = true
    
    SE match encontrado:
        Adicionar ReconciliationItem(tx, match, MATCHED)
        Marcar match como usado
    SENÃO:
        Adicionar ReconciliationItem(tx, null, MISSING)

PARA CADA externalEntry não usado:
    Adicionar ReconciliationItem(null, ext, EXTRA)

RETORNAR lista de ReconciliationItems
```

### Critérios de Match (DefaultReconciliationPolicy)

| Critério | Regra |
|----------|-------|
| **Reference** | Exatamente igual |
| **Date** | Mesmo dia (ignora hora) |
| **Amount** | Diferença ≤ tolerância |

## 📁 Estrutura do Projeto

```
Conciliacao/
├── Conciliacao.Api/              # Web API (ASP.NET Core)
├── Conciliacao.Application/      # Casos de uso e orquestração
├── Conciliacao.Domain/           # Núcleo do domínio
│   ├── Entities/
│   │   ├── Transaction.cs        # Transação interna
│   │   ├── ExternalEntry.cs      # Entrada externa (banco, gateway)
│   │   └── ReconciliationItem.cs # Resultado da conciliação
│   ├── ValueObjects/
│   │   └── Money.cs              # Comparação monetária c/ tolerância
│   ├── Enums/
│   │   └── ReconciliationResult.cs
│   ├── Policies/
│   │   ├── IReconciliationPolicy.cs
│   │   └── DefaultReconciliationPolicy.cs
│   └── Services/
│       └── SimpleReconciliationService.cs
├── Conciliacao.Infra/            # Persistência e integrações
└── Conciliacao.Domain.Tests/     # Testes unitários
```

## 🚀 Quick Start

### Pré-requisitos

- .NET 10 SDK
- Git

### Instalação e Execução

```bash
# Clone o repositório
git clone https://github.com/awernek/Conciliacao.git
cd Conciliacao

# Restaurar dependências e compilar
dotnet build

# Executar testes
dotnet test

# Executar a aplicação (quando disponível)
dotnet run --project Conciliacao.Api
```

### Exemplo de Uso

```csharp
using Conciliacao.Domain.Policies;
using Conciliacao.Domain.Services;
using Conciliacao.Domain.Enums;

// 1. Configurar a política de reconciliação
var policy = new DefaultReconciliationPolicy(tolerance: 0.01m);

// 2. Criar o serviço de reconciliação
var service = new SimpleReconciliationService(policy);

// 3. Executar a reconciliação
var results = service.Reconcile(transactions, externalEntries);

// 4. Analisar resultados
var matched = results.Count(r => r.Result == ReconciliationResult.Matched);
var missing = results.Count(r => r.Result == ReconciliationResult.Missing);
var extra   = results.Count(r => r.Result == ReconciliationResult.Extra);

Console.WriteLine($"Matched: {matched}, Missing: {missing}, Extra: {extra}");
```

## 💡 Decisões de Design

| Decisão | Motivação |
|---------|-----------|
| **Strategy Pattern** (Policies) | Permite trocar regras de matching sem alterar o serviço principal. Facilita a adição de novas políticas de reconciliação. |
| **Value Object Money** | Encapsula comparação monetária com tolerância, evitando erros comuns de ponto flutuante e centralizando lógica de comparação. |
| **Imutabilidade em ReconciliationItem** | Resultados são read-only após criação, garantindo consistência e facilitando debugging. |
| **HashSet para tracking** | Complexidade O(1) para verificar entries já usados, otimizando performance em grandes volumes. |
| **Separação Transaction/ExternalEntry** | Origens distintas = entidades distintas. Respeita bounded contexts e facilita evolução independente. |
| **Interface IReconciliationPolicy** | Inversão de dependência (SOLID), permitindo injeção de diferentes estratégias sem acoplamento. |

## 🔧 Extensibilidade

### Criando Políticas Customizadas

Implemente a interface `IReconciliationPolicy` para criar regras de matching personalizadas:

```csharp
public class StrictPolicy : IReconciliationPolicy
{
    public bool IsMatch(Transaction tx, ExternalEntry ext)
    {
        return tx.Reference == ext.Reference 
            && tx.Date.Date == ext.Date.Date 
            && tx.Amount == ext.Amount; // Sem tolerância
    }
}
```

### Exemplo: Política com Fuzzy Matching

```csharp
public class FuzzyReferencePolicy : IReconciliationPolicy
{
    private readonly decimal _tolerance;
    private readonly int _levenshteinThreshold;

    public FuzzyReferencePolicy(decimal tolerance = 0.01m, int levenshteinThreshold = 3)
    {
        _tolerance = tolerance;
        _levenshteinThreshold = levenshteinThreshold;
    }

    public bool IsMatch(Transaction tx, ExternalEntry ext)
    {
        var referenceMatch = LevenshteinDistance(tx.Reference, ext.Reference) 
                             <= _levenshteinThreshold;
        var dateMatch = tx.Date.Date == ext.Date.Date;
        var amountMatch = Math.Abs(tx.Amount - ext.Amount) <= _tolerance;

        return referenceMatch && dateMatch && amountMatch;
    }

    private int LevenshteinDistance(string s1, string s2) 
    {
        // Implementação do algoritmo de Levenshtein
        // ...
    }
}
```

## 🗺️ Roadmap

### Versão 1.0
- [ ] API REST completa com endpoints CRUD
- [ ] Persistência com Entity Framework Core
- [ ] Documentação OpenAPI/Swagger
- [ ] Containerização com Docker

### Versão 2.0
- [ ] Suporte multi-moeda com conversão automática
- [ ] Processamento assíncrono com mensageria (RabbitMQ/Kafka)
- [ ] Dashboard interativo de visualização
- [ ] Exportação de relatórios (PDF, Excel)

### Versão 3.0
- [ ] Machine Learning para sugestões de matching
- [ ] Auditoria completa de alterações
- [ ] Notificações em tempo real
- [ ] Integração com sistemas externos via webhooks

## 📄 Licença

Este projeto está licenciado sob a Licença MIT - veja o arquivo [LICENSE](LICENSE) para detalhes.

## 👤 Autor

**Anderson Wernek**
- GitHub: [@awernek](https://github.com/awernek)

## 🤝 Contribuindo

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou enviar pull requests.

1. Fork o projeto
2. Crie sua feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

---

⭐ Se este projeto foi útil para você, considere dar uma estrela no repositório!