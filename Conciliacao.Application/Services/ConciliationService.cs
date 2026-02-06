using Conciliacao.Application.Requests;
using Conciliacao.Application.Results;
using Conciliacao.Domain.Entities;
using Conciliacao.Domain.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Conciliacao.Application.Services
{
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
        // 1️⃣ Constrói as entidades de domínio (somente memória)
        var transactions = request.Items
            .Select(item => new Transaction("", item.Reference, item.Amount, DateTime.UtcNow))
            .ToList();

        // 2️⃣ Calcula o resultado de forma determinística
        // IMPORTANTE: o mesmo input SEMPRE gera o mesmo hash
        var result = ConciliationResult.SuccessResult(transactions.Count);

        try
        {
            // 3️⃣ Persiste as transações
            await _transactionRepository.AddRangeAsync(transactions);

            // 4️⃣ Registra a chave de idempotência
            // Aqui mora a concorrência:
            // - Existe índice UNIQUE no banco
            // - Duas requisições simultâneas podem chegar aqui
            // - Apenas uma irá conseguir inserir
            var processedRequest = new ProcessedRequest(
                idempotencyKey,
                result.ToPayload()
            );

            await _processedRequestRepository.AddAsync(processedRequest);

            // 5️⃣ Commit ÚNICO
            // Se qualquer coisa falhar acima, nada é salvo
            await _unitOfWork.CommitAsync();

            return result;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // 6️⃣ Outro request venceu a corrida
            // O banco garantiu a consistência, agora só reagimos

            var processed = await _processedRequestRepository
                .GetByKeyAsync(idempotencyKey);

            if (processed is null)
            {
                // Situação raríssima (e grave)
                throw new InvalidOperationException(
                    "Idempotency conflict detected, but processed request was not found.");
            }

            // 7️⃣ Reconstrói o resultado a partir do payload salvo (idempotência: mesmo resultado)
            return ConciliationResult.FromPayload(processed.ResultHash);
        }
    }

    /// <summary>
    /// SQL Server:
    /// 2601 → Duplicate key row
    /// 2627 → Violation of UNIQUE constraint
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqlException sqlEx
            && (sqlEx.Number == 2601 || sqlEx.Number == 2627);
    }
}
}