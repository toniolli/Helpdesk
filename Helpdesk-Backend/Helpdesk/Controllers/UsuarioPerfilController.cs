using Application.Dto.UsuarioDto;
using Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Helpdesk.Authorization;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Consulta e mantém os vínculos entre usuários e perfis.
    /// </summary>
    public class UsuarioPerfilController : ControllerBase
    {
        private readonly IUsuarioPerfilService _usuarioPerfilService;

        public UsuarioPerfilController(IUsuarioPerfilService usuarioPerfilService)
        {
            _usuarioPerfilService = usuarioPerfilService;
        }

        // CONSULTAS

        [HttpGet("usuario/{usuarioId}")]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200, Type = typeof(UsuarioPerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioPerfilDto>> BuscarPerfilUsuario(string perfilUsuarioId)
        {
            if (perfilUsuarioId == null)
                return BadRequest("Usuario perfil inválida");

            var perfilPermissao = await _usuarioPerfilService.BuscarPerfisUsuario(perfilUsuarioId);

            if (perfilPermissao == null)
                return NotFound("Usuario perfil  não encontrado");

            return Ok(perfilPermissao);
        }

        [HttpGet("perfil/{perfilId}")]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200, Type = typeof(UsuarioPerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<UsuarioPerfilDto>> BuscarUsuarioPerfill(int usuarioPerfilId)
        {
            if (usuarioPerfilId <= 0)
                return BadRequest("Usuario perfil  inválida");

            var perfilPermissao = await _usuarioPerfilService.BuscarUsuariosPerfil(usuarioPerfilId);

            if (perfilPermissao == null)
                return NotFound("Usuario perfil   não encontrado");

            return Ok(perfilPermissao);
        }

        // AÇÕES

        [HttpPost]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200, Type = typeof(UsuarioPerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Vincular([FromBody] UsuarioPerfilDto usuarioPerfilDto)
        {
            if (usuarioPerfilDto == null)
                return BadRequest();

            await _usuarioPerfilService.Vincular(usuarioPerfilDto);

            return Ok(usuarioPerfilDto);
        }

        [HttpDelete("{usuarioId}/{perfilId}")]
        [Permissao("GERENCIAR_PERFIS")]
        [ProducesResponseType(200, Type = typeof(UsuarioPerfilDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Desvincular(string usuarioId, int perfilID)
        {
            if (perfilID <= 0)
                return BadRequest("Código inválido");

            if (usuarioId == null)
                return BadRequest("Código inválido");

            await _usuarioPerfilService.Desvincular(usuarioId, perfilID);

            return Ok();
        }
    }
}
