using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ReconciliationController : ControllerBase
{
    private readonly IReconciliationAppService _appService;
    private readonly ILogger<ReconciliationController> _logger;

    public ReconciliationController(
        IReconciliationAppService appService,
        ILogger<ReconciliationController> logger)
    {
        _appService = appService;
        _logger = logger;
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar conciliação em lote para clientCode={ClientCode}", clientCode);
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}