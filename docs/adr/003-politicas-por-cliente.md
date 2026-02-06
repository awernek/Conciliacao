# 3. Políticas de Conciliação por Cliente

Data: 2026-02-06
Status: Aceito

## Contexto
Diferentes clientes possuem regras distintas para considerar uma transação como "conciliada" (Matched). Alguns aceitam tolerância de valor, outros exigem data exata, outros ignoram data. Hardcoded `if/else` tornaria o código inmanutenível.

## Decisão
Utilizamos os padrões **Strategy**, **Composite** e **Factory**:

1. **Strategy (`IConciliationPolicy`)**: O serviço de conciliação desconhece as regras concretas, apenas chama `IsMatch()`.
2. **Composite (`CompositeConciliationPolicy`)**: Permite combinar múltiplas regras atômicas (`ReferenceMatch`, `DateMatch`, `AmountTolerance`) em uma política única que exige que *todas* sejam verdadeiras.
3. **Factory (`ConciliationPolicyFactory`)**: Centraliza a criação da política correta baseada no código do cliente (`clientCode`).

## Consequências
- **Positivo**: Adicionar um novo cliente com regras existentes é apenas uma nova linha na Factory.
- **Positivo**: Regras são testáveis isoladamente.
- **Negativo**: A complexidade da criação de objetos aumenta (Factories necessárias).
