using Conciliacao.Application.DTOs;
using Conciliacao.Application.DTOs.Reconciliation;
using Conciliacao.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Conciliacao.Api.Controllers
{
    [ApiController]
    [Route("api/reconciliation")]
    public class ReconciliationController : ControllerBase
    {
        private readonly ReconciliationAppService _service;

        public ReconciliationController(ReconciliationAppService service)
        {
            _service = service;
        }

        [HttpPost("batch")]
        [ProducesResponseType(typeof(ReconciliationBatchResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult ReconcileBatch(
            [FromBody] ReconciliationBatchRequestDto request)
        {
            if (request == null)
                return BadRequest("Request inválido");

            var result = _service.ReconcileBatch(request);

            return Ok(result);
        }
    }
}