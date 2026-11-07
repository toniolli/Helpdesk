using Application.Dto.Chamado;
using Application.Dto.RegraDistribuicaoDto;
using Application.DTOs;
using Application.Interfaces;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Gerencia regras, critérios e aplicações da distribuição de chamados.
    /// </summary>
    public class RegraDistribuicaoController : ControllerBase
    {
        private readonly IRegraDistribuicaoService _regraService;

        public RegraDistribuicaoController(
            IRegraDistribuicaoService regraService)
        {
            _regraService = regraService;
        }

        // CONSULTAS

        [HttpGet("{id}", Name = "GetRegraDistribuicaoPorId")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<RegraDistribuicaoResponseDto>>
            BuscarPorId(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            var regra = await _regraService.BuscarPorId(id);

            if (regra == null)
                return NotFound("Regra não encontrada.");

            return Ok(regra);
        }

        [HttpGet]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<RegraDistribuicaoResponseDto>>>
            BuscarTodos()
        {
            var regras = await _regraService.BuscarTodos();

            if (regras == null || !regras.Any())
                return NotFound("Nenhuma regra encontrada.");

            return Ok(regras);
        }

        [HttpGet("{id}/chamados-disponiveis")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChamadoResponseDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<ChamadoResponseDto>>>
        BuscarChamadosDisponiveis(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamados = await _regraService
                .BuscarChamadosDisponiveis(id);

            if (chamados == null || !chamados.Any())
                return NotFound("Nenhum chamado disponível encontrado.");

            return Ok(chamados);
        }

        // AÇÕES

        // Manutenção da regra

        [HttpPost]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult>
            Adicionar([FromBody] RegraDistribuicaoRequestDto regraDto)
        {
            if (regraDto == null)
                return BadRequest();

            var regra = await _regraService.Adicionar(regraDto);

            return Ok(regra);
        }

        [HttpPut("{id}")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Atualizar(
                int id,
                [FromBody] RegraDistribuicaoRequestDto regraDto)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            if (regraDto == null)
                return BadRequest();

            var regra = await _regraService.Atualizar(id, regraDto);

            return Ok(regra);
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

            await _regraService.Remover(id);

            return Ok();
        }

        [HttpPut("{id}/ativar")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Ativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _regraService.Ativar(id);

            return Ok("Regra ativada com sucesso.");
        }

        [HttpPut("{id}/desativar")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Desativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _regraService.Desativar(id);

            return Ok("Regra desativada com sucesso.");
        }

        // Critérios da regra

        [HttpPost("{id}/criterios/{criterioId}")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            AdicionarCriterio(
                int id,
                CriterioDistribuicaoRequestDto criterio)
        {
            if (id <= 0)
                return BadRequest("Id da regra inválido.");



            var regra =
                await _regraService.AdicionarCriterio(id, criterio);

            return Ok(regra);
        }

        [HttpDelete("{id}/criterios/{criterioId}")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            RemoverCriterio(int criterioId)
        {
         
            if (criterioId <= 0)
                return BadRequest("Id do critério inválido.");

            await _regraService.RemoverCriterio(criterioId);

            return Ok();
        }

        // Aplicação da regra em chamados

        [HttpPost("{id}/aplicar-chamados")]
        [Permissao("GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> AplicarRegraChamados(int id,[FromBody] AplicarRegraChamadosRequestDto request)
        {
            if (id <= 0)
                return BadRequest("Id da regra inválido.");

            if (request == null)
                return BadRequest("Dados para aplicação da regra são obrigatórios.");

            request.RegraId = id;

            await _regraService.AplicarRegraChamados(request);

            return NoContent();
        }

    }
}
