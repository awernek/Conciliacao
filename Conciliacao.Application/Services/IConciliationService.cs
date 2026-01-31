using Conciliacao.Application.Requests;
using Conciliacao.Application.Results;

namespace Conciliacao.Application.Services
{
    public interface IConciliationService
    {
        Task<ConciliationResult> ConciliateAsync(
        ConciliationRequest request,
        string idempotencyKey);

    }
}
