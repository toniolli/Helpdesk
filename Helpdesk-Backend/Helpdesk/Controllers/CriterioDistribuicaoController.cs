using Application.DTOs;
using Application.Interfaces;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Gerencia os critérios utilizados pelas regras de distribuição.
    /// </summary>
    public class CriterioDistribuicaoController : ControllerBase
    {
        private readonly ICriterioDistribuicaoService _criterioService;

        public CriterioDistribuicaoController(ICriterioDistribuicaoService criterioService)
        {
            _criterioService = criterioService;
        }

        // CONSULTAS

        [HttpGet("{id}", Name = "GetCriterioDistribuicaoPorId")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CriterioDistribuicaoResponseDto>>
            BuscarPorId(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            var criterio = await _criterioService.BuscarPorId(id);

            if (criterio == null)
                return NotFound("Critério não encontrado.");

            return Ok(criterio);
        }

        [HttpGet]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<CriterioDistribuicaoResponseDto>>>
            BuscarTodos()
        {
            var criterios = await _criterioService.BuscarTodos();

            if (criterios == null || !criterios.Any())
                return NotFound("Nenhum critério encontrado.");

            return Ok(criterios);
        }

        [HttpGet("{id}/criterios")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<CriterioDistribuicaoResponseDto>>>
            BuscarCriteriosPorRegra(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            var criterios =
                await _criterioService.BuscarCriteriosPorRegra(id);

            return Ok(criterios);
        }

        // AÇÕES

        [HttpPost]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult>
            Adicionar([FromBody] CriterioDistribuicaoRequestDto criterioDto)
        {
            if (criterioDto == null)
                return BadRequest();

            var criterio = await _criterioService.Adicionar(criterioDto);

            return Ok(criterio);
        }

        [HttpDelete("{id}")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Remover(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _criterioService.Remover(id);

            return Ok();
        }

    }
}
