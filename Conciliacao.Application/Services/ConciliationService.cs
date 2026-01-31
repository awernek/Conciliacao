using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Conciliacao.Application.Requests;
using Conciliacao.Application.Results;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;

public class ConciliationService : IConciliationService
{
    private readonly IProcessedRequestRepository _processedRequestRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConciliationService(
        IProcessedRequestRepository processedRequestRepository,
        ITransactionRepository transactionRepository,
        IUnitOfWork unitOfWork)
    {
        _processedRequestRepository = processedRequestRepository;
        _transactionRepository = transactionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ConciliationResult> ConciliateAsync(
        ConciliationRequest request,
        string idempotencyKey)
    {
        // ⚙️ 1. Constrói as entidades de domínio
        // Aqui NÃO tocamos banco ainda
        var transactions = request.ToTransactions();

        // 🧮 2. Calcula o resultado da conciliação
        // Resultado é determinístico → pode ser reconstruído
        var result = ConciliationResult.SuccessResult(transactions.Count);

        try
        {
            // 💾 3. Persiste as transações
            await _transactionRepository.AddRangeAsync(transactions);

            // 🔐 4. Persiste a chave de idempotência
            // IMPORTANTE:
            // Existe um índice UNIQUE no banco para IdempotencyKey
            // Se duas requisições concorrentes tentarem salvar a mesma chave,
            // o banco irá garantir exclusividade
            await _processedRequestRepository.AddAsync(
                new ProcessedRequest(idempotencyKey, result.ToHash())
            );

            // ✅ 5. Commit único (transação atômica)
            await _unitOfWork.CommitAsync();

            return result;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // 🔁 6. Caso clássico de idempotência:
            // Outra requisição já processou essa chave antes
            // Recuperamos o resultado persistido e devolvemos
            var processed = await _processedRequestRepository
                                          .GetByKeyAsync(idempotencyKey);

            if (processed == null)
            {
                throw new InvalidOperationException(
                    "Idempotency conflict detected but processed request was not found.");
            }

            return ConciliationResult.FromHash(processed.ResultHash);
        }
    }

    /// <summary>
    /// Detecta violação de índice UNIQUE no SQL Server
    /// 2601 → Cannot insert duplicate key row
    /// 2627 → Violation of UNIQUE constraint
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx
            && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}