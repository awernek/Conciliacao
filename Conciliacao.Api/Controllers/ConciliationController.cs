using Conciliacao.Application.Requests;
using Conciliacao.Application.Results;
using Conciliacao.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conciliacao.Api.Controllers
{
    /// <summary>
    /// Endpoint de conciliação com idempotência.
    /// Obrigatório enviar o header Idempotency-Key; requisições com a mesma chave retornam o mesmo resultado (sem reprocessar).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ConciliationController : ControllerBase
    {
        private const string IdempotencyKeyHeaderName = "Idempotency-Key";
        private readonly IConciliationService _conciliationService;
        private readonly ILogger<ConciliationController> _logger;

        public ConciliationController(
            IConciliationService conciliationService,
            ILogger<ConciliationController> logger)
        {
            _conciliationService = conciliationService;
            _logger = logger;
        }

        /// <summary>
        /// Processa a conciliação de forma idempotente.
        /// Envie o header Idempotency-Key (ex.: GUID ou valor único por operação).
        /// Se a mesma chave for enviada de novo, a API retorna o resultado já salvo sem reprocessar.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(ConciliationResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ConciliationResult>> Conciliate(
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
                _logger.LogError(ex, "Erro ao processar conciliação idempotente para key={IdempotencyKey}", idempotencyKey);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}
