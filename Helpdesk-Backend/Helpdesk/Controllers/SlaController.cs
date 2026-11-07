
using Application.Dto.SlaDto;
using Application.Interfaces;
using Domain.Entities;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Gerencia configurações de SLA e expõe seus cálculos por chamado.
    /// </summary>
    public class SlaController : ControllerBase
    {
        private readonly ISlaService _slaService;

        public SlaController(ISlaService slaService)
        {
            _slaService = slaService;
        }

        // CONSULTAS

        [HttpGet("{id}", Name = "GetSlaPorId")]
        [Permissao("VISUALIZAR_SLA")]
        [ProducesResponseType(200, Type = typeof(SlaResponseDto))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SlaResponseDto>> BuscarPorId(int id)
        {
            if (id <= 0)
                return BadRequest("SLA inválido.");

            var sla = await _slaService.BuscarPorId(id);

            if (sla == null)
                return NotFound("SLA não encontrado.");

            return Ok(sla);
        }


        [HttpGet("prioridade/{prioridade}")]
        [Permissao("VISUALIZAR_SLA")]
        [ProducesResponseType(200, Type = typeof(SlaResponseDto))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<SlaResponseDto>>BuscarPorPrioridade( PrioridadeEnum prioridade)
        {
            var sla =
                await _slaService.BuscarPrioridade(
                    prioridade);

            if (sla == null)
                return NotFound(
                    "SLA não encontrado.");

            return Ok(sla);
        }



        [HttpGet]
        [Permissao("VISUALIZAR_SLA")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<SlaResponseDto>))]
        public async Task<ActionResult<IEnumerable<SlaResponseDto>>> BuscarTodos()
        {
            var slas = await _slaService.BuscarTodos();

            if (!slas.Any())
                return NotFound("Nenhum SLA encontrado.");

            return Ok(slas);
        }

        [HttpGet("chamado/{chamadoId}/tempo-atendimento/{slaId}")]
        [Permissao("VISUALIZAR_SLA")]
        [ProducesResponseType(200, Type = typeof(SlaResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SlaResponseDto>>CalcularTempoAtendimento(int chamadoId,int slaId)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar =
                User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (chamadoId <= 0)
                return BadRequest("Chamado inválido.");

            if (slaId <= 0)
                return BadRequest("SLA inválido.");

            var resultado =
                await _slaService.CalcularTempoAtendimento(
                    chamadoId,
                    slaId,
                    usuarioLogadoId,
                    podeVisualizar);

            return Ok(resultado);
        }

        [HttpGet("chamado/{chamadoId}/tempo-resolucao/{slaId}")]
        [Permissao("VISUALIZAR_SLA")]
        [ProducesResponseType(200, Type = typeof(SlaResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<SlaResponseDto>>CalcularTempoResolucao(int chamadoId, int slaId)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar =
                User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (chamadoId <= 0)
                return BadRequest("Chamado inválido.");

            if (slaId <= 0)
                return BadRequest("SLA inválido.");

            var resultado =
                await _slaService.CalcularTempoResolucao(
                    chamadoId,
                    slaId,
                    usuarioLogadoId,
                    podeVisualizar);

            return Ok(resultado);
        }

        // AÇÕES

        [HttpPost]
        [Permissao("CONFIGURAR_SLA")]
        [ProducesResponseType(200, Type = typeof(SlaResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Criar(
            [FromBody] SlaRequestDto dto)
        {
            var sla = await _slaService.Adicionar(dto);

            return Ok(sla);
        }

        [HttpPut("{id}")]
        [Permissao("CONFIGURAR_SLA")]
        [ProducesResponseType(200, Type = typeof(SlaResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Atualizar(int id,[FromBody] SlaRequestDto dto)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            var sla =
                await _slaService.Atualizar(id, dto);

            return Ok(sla);
        }

        [HttpPut("{id}/ativar")]
        [Permissao("CONFIGURAR_SLA")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Ativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _slaService.Ativar(id);

            return Ok();
        }

        [HttpPut("{id}/desativar")]
        [Permissao("CONFIGURAR_SLA")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Desativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _slaService.Desativar(id);

            return Ok();
        }

        [HttpDelete("{id}")]
        [Permissao("CONFIGURAR_SLA")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Remover(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _slaService.Remover(id);

            return Ok();
        }

    }
}
