using Application.Dto.UsuarioDto;
using Application.Interfaces;
using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Helpdesk.Authorization;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Gerencia os perfis de acesso e suas configurações.
    /// </summary>
    public class PerfilController : ControllerBase
    {
        private readonly IPerfilService _perfilService;

        public PerfilController(IPerfilService perfilService)
        {
            _perfilService = perfilService;
        }

        // CONSULTAS

        [HttpGet("opcoes-filtro")]
        [Permissao("VISUALIZAR_TODOS_CHAMADOS")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(IEnumerable<PerfilOpcaoFiltroDto>))]
        public async Task<ActionResult<IEnumerable<PerfilOpcaoFiltroDto>>>
            BuscarOpcoesFiltro()
        {
            var perfis = await _perfilService.BuscarTodos();

            var opcoes = perfis.Select(perfil =>
                new PerfilOpcaoFiltroDto
                {
                    Id_Perfil = perfil.Id_Perfil.GetValueOrDefault(),
                    Nome = perfil.Nome
                });

            return Ok(opcoes);
        }

        [HttpGet("{id}", Name = "GetPerfilPorId")]
        [Permissao("GERENCIAR_PERFIS", "GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200, Type = typeof(PerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PerfilDto>> BuscarPorId(int id)
        {
            if (id <= 0)
                return BadRequest("perfil inválido");

            var perfil = await _perfilService.BuscarPorId(id);

            if (perfil == null)
                return NotFound("Perfil não encontrado");

            return Ok(perfil);
        }

        [HttpGet]
        [Permissao("GERENCIAR_PERFIS", "GERENCIAR_DISTRIBUICAO")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<PerfilDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<PerfilDto>>> BuscarTodos()
        {
            var perfil = await _perfilService.BuscarTodos();

            if (perfil == null)
                return NotFound("Nenhum perfil encontrado");

            return Ok(perfil);
        }

        // AÇÕES

        [HttpPost]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200, Type = typeof(PerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PerfilDto>> Adicionar(
            [FromBody] PerfilDto perfilDto)
        {
            if (perfilDto == null)
                return BadRequest();

            var perfilCriado = await _perfilService.Adicionar(
                perfilDto);

            return Ok(perfilCriado);
        }

        [HttpPut("{id}")]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200, Type = typeof(PerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PerfilDto>> Atualizar(
            int id,
            [FromBody] PerfilDto perfilDto)
        {
            if (id <= 0)
                return BadRequest("Id inválido");

            if (perfilDto == null)
                return BadRequest();

            var perfilAtualizado = await _perfilService.Atualizar(
                id,
                perfilDto);

            return Ok(perfilAtualizado);
        }

        [HttpDelete("{id}")]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200, Type = typeof(PerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Remover(int id)
        {
            if (id <= 0)
                return BadRequest("Código inválido");

            await _perfilService.Remover(id);

            return Ok();
        }


        [HttpPut("{id}/ativar")]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Ativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _perfilService.Ativar(id);

            return Ok("Perfil ativada com sucesso.");
        }

        [HttpPut("{id}/desativar")]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Desativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _perfilService.Desativar(id);

            return Ok("Perfil desativada com sucesso.");
        }
    }
}
