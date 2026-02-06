using Conciliacao.Application.DTOs.Conciliation;
using Conciliacao.Application.Requests;
using Conciliacao.Application.Results;
using Conciliacao.Application.Services;
using Conciliacao.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Conciliacao.Api.Controllers
{
    /// <summary>
    /// API de Conciliação com dois fluxos:
    /// - POST: conciliação com idempotência (header Idempotency-Key obrigatório).
    /// - POST batch: conciliação em lote (sem idempotência).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ConciliationController : ControllerBase
    {
        private const string IdempotencyKeyHeaderName = "Idempotency-Key";
        private readonly IConciliationService _conciliationService;
        private readonly IConciliationBatchService _conciliationBatchService;
        private readonly ILogger<ConciliationController> _logger;

        public ConciliationController(
            IConciliationService conciliationService,
            IConciliationBatchService conciliationBatchService,
            ILogger<ConciliationController> logger)
        {
            _conciliationService = conciliationService;
            _conciliationBatchService = conciliationBatchService;
            _logger = logger;
        }

        /// <summary>
        /// Conciliação com idempotência. Envie o header Idempotency-Key.
        /// Requisições com a mesma chave retornam o mesmo resultado (sem reprocessar).
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ConciliationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConciliationResult>> Post(
            [FromHeader(Name = IdempotencyKeyHeaderName)] string? idempotencyKey,
            [FromBody] ConciliationRequest request)
        {
            if (string.IsNullOrWhiteSpace(idempotencyKey))
            {
                return BadRequest($"Header '{IdempotencyKeyHeaderName}' é obrigatório para garantir idempotência.");
            }

            try
            {
                var result = await _conciliationService.ConciliateAsync(request, idempotencyKey.Trim());
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar conciliação (com idempotência) para key={IdempotencyKey}", idempotencyKey);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Conciliação em lote (sem idempotência). Envie clientCode na query e Transactions/ExternalEntries no body.
        /// </summary>
        [HttpPost("batch")]
        [ProducesResponseType(typeof(ConciliationBatchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConciliationBatchResponseDto>> PostBatch(
            [FromQuery] string clientCode,
            [FromBody] ConciliationBatchRequestDto request)
        {
            try
            {
                var client = new Client(clientCode);
                var result = await _conciliationBatchService.ConciliateBatchAsync(client, request.Transactions, request.ExternalEntries);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar conciliação em lote para clientCode={ClientCode}", clientCode);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
