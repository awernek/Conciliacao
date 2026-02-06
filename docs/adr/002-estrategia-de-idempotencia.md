# 2. Estratégia de Idempotência e Concorrência

Data: 2026-02-06
Status: Aceito

## Contexto
A API de conciliação recebe requisições financeiras críticas. Se houver falha de rede na resposta, o cliente pode tentar reenviar (retry). O sistema não pode processar a mesma conciliação duas vezes (duplicar registros).

## Decisão
Implementamos idempotência baseada em chave única (Header `Idempotency-Key`):

1. **Tabela `ProcessedRequests`**: Armazena a chave (`IdempotencyKey`), hash do resultado e data.
2. **Índice UNIQUE**: O banco de dados garante a unicidade da chave.
3. **Tratamento de Exceção**: O `UnitOfWork` captura a violação de chave única (`DbUpdateException` com códigos 2601/2627 do SQL Server) e lança uma `DuplicateKeyException` (Domain Exception).
4. **Fluxo**: Ao capturar `DuplicateKeyException`, a aplicação ignora a escrita e retorna o resultado original salvo em `ProcessedRequests`.

## Consequências
- **Positivo**: Garante consistência forte (o banco é a fonte de verdade).
- **Positivo**: Protege contra "Double Spending" ou processamento duplo.
- **Negativo**: Exige que o cliente gerencie e envie chaves únidas (GUIDs).
- **Desafio**: O tratamento de exceção depende de códigos específicos do banco (SQL Server), encapsulados no `UnitOfWork` para não poluir o Application.
