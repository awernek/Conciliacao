using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReconciliationController : ControllerBase
{
    private readonly ReconciliationAppService _appService;

    public ReconciliationController(ReconciliationAppService appService)
    {
        _appService = appService;
    }

    [HttpPost("batch")]
    public async Task<ActionResult<ReconciliationBatchResponseDto>> ReconcileBatch(
        [FromQuery] string clientCode,
        [FromBody] BatchReconciliationRequestDto request)
    {
        try
        {
            var client = new Client { Code = clientCode };
            var result = await _appService.ReconcileBatchAsync(client, request.Transactions, request.ExternalEntries);
            return Ok(result);
        }
        catch
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}