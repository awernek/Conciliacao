# Testes de Domínio (Conciliacao.Domain.Tests)

Este projeto contém os testes unitários da camada de domínio da aplicação de Conciliação.

## Estrutura

```
Conciliacao.Domain.Tests/
├── README.md                           # Este arquivo
├── CODE_REVIEW.md                      # Revisão e convenções dos testes
├── FakeReconciliationPolicyFactory.cs  # Fixture para testes que dependem de IReconciliationPolicyFactory
├── MoneyTests.cs                       # ValueObject Money
├── DefaultReconciliationPolicyTests.cs # Política padrão de conciliação
├── SimpleReconciliationServiceTests.cs # Serviço de conciliação (domínio)
├── ReconciliationAppServiceTests.cs    # Serviço de aplicação (batch)
├── ReconciliationAppServiceFlowTests.cs # Fluxos de aplicação com factory real
└── Policies/
    ├── CompositeReconciliationPolicyTests.cs
    └── Rules/
        ├── AmountToleranceRuleTests.cs
        ├── DateMatchRuleTests.cs
        └── ReferenceMatchRuleTests.cs
```

## Convenções

- **Padrão de nome de testes**: `Method_Should_Result_When_Condition` (ex.: `IsMatch_Should_Return_True_When_Amount_Is_Within_Tolerance`).
- **Preparar/Agir/Verificar**: uso explícito de comentários `// Preparar`, `// Agir`, `// Verificar` em testes mais longos.
- **Namespace**: espelha a estrutura de pastas; testes de regras em `Conciliacao.Domain.Tests.Policies.Rules`.
- **Documentação**: testes de contrato ou edge cases importantes podem ter `<summary>` e `<remarks>` em XML.

## Dependências

- xUnit
- Microsoft.NET.Test.Sdk
- coverlet.collector (cobertura)
- Referências: Conciliacao.Domain, Conciliacao.Application (para testes de aplicação hospedados aqui)

## Execução

```bash
dotnet test Conciliacao.Domain.Tests/Conciliacao.Domain.Tests.csproj
```

Com cobertura (se configurado no projeto):

```bash
dotnet test Conciliacao.Domain.Tests/Conciliacao.Domain.Tests.csproj --collect:"XPlat Code Coverage"
```
