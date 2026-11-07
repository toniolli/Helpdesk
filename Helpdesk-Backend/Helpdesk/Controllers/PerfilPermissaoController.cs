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
    /// Consulta e mantém os vínculos entre perfis e permissões.
    /// </summary>
    public class PerfilPermissaoController : ControllerBase
    {
        private readonly IperfilPermissaoService _perfilPermissaoService;

        public PerfilPermissaoController(IperfilPermissaoService perfilPermissaoService)
        {
            _perfilPermissaoService = perfilPermissaoService;
        }

        // CONSULTAS

        [HttpGet("permissao/{permissaoId}")]
        [Permissao("GERENCIAR_PERMISSOES")]
        [ProducesResponseType(200, Type = typeof(PerfilPermissaoDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PerfilPermissaoDto>> BuscarPermissaoPerfil(int permissaoId)
        {
            var perfis = await _perfilPermissaoService.BuscarPerfisPermissao(permissaoId);
            return Ok(perfis);

        }

        [HttpGet("perfil/{perfilId}")]
        [Permissao("GERENCIAR_PERMISSOES")]
        [ProducesResponseType(200, Type = typeof(PerfilPermissaoDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<PerfilPermissaoDto>> BuscarPerfilPermissao(int perfilId)
        {
            var permissoes = await _perfilPermissaoService.BuscarPermissoesPerfil(perfilId);
            return Ok(permissoes);
        }

        // AÇÕES

        [HttpPost]
        [Permissao("GERENCIAR_PERMISSOES")]
        [ProducesResponseType(200, Type = typeof(PerfilPermissaoDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Vincular([FromBody] PerfilPermissaoDto perfilPermissaoDto)
        {
            if (perfilPermissaoDto == null)
                return BadRequest();

            await _perfilPermissaoService.Vincular(perfilPermissaoDto);

            return Ok(perfilPermissaoDto);
        }

        [HttpDelete("{perfilId}/{permissaoId}")]
        [Permissao("GERENCIAR_PERMISSOES")]
        [ProducesResponseType(200, Type = typeof(PerfilPermissaoDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Desvincular(int perfilId, int permissaoId)
        {
            if (perfilId <= 0 && permissaoId <= 0)
                return BadRequest("Código inválido");

            await _perfilPermissaoService.Desvincular(perfilId, permissaoId);

            return Ok();
        }
    }
}
