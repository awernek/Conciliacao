# 4. Estratégia de Persistência em Lote (Batch)

Data: 2026-02-06
Status: Aceito

## Contexto
O fluxo de conciliação em lote recebe grandes volumes de transações. Processar e salvar um a um seria ineficiente e arriscado (estado parcial se falhar no meio).

## Decisão
Adotamos o padrão **Unit of Work** para persistência atômica do "Aggregate" lógico da conciliação:

1. O `ConciliationBatchService` orquestra todo o processo na memória.
2. Persiste Transaction, ExternalEntries e calcula o resultado.
3. Chama `UnitOfWork.CommitAsync()` **apenas uma vez, no final** do fluxo.
4. Qualquer erro antes do final resulta em rollback implícito (nada é salvo).

## Consequências
- **Positivo**: Integridade dos dados (Tudo ou Nada).
- **Positivo**: Performance (menos round-trips ao banco).
- **Negativo**: Consumo de memória maior para batches gigantes (mas aceitável para o escopo atual).
