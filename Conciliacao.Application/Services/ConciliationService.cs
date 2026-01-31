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
        // 🔎 1. Já foi processado?
        var processed = await _processedRequestRepository
            .GetByKeyAsync(idempotencyKey);

        if (processed != null)
        {
            return ConciliationResult.FromHash(processed.ResultHash);
        }

        // ⚙️ 2. Processa normalmente
        var transactions = request.ToTransactions();

        await _transactionRepository.AddRangeAsync(transactions);

        var result = ConciliationResult.SuccessResult(transactions.Count);

        // 💾 3. Salva idempotência
        await _processedRequestRepository.AddAsync(
            new ProcessedRequest(idempotencyKey, result.ToHash())
        );

        // ✅ 4. Commit único
        await _unitOfWork.CommitAsync();

        return result;
    }
}
