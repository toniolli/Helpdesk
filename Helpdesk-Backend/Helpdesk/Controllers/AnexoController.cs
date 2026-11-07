
using Application.DTOs.Anexo;
using Application.Interfaces;
using Helpdesk.Authorization;
using Infra.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Consulta, armazena, remove e disponibiliza anexos dos chamados.
    /// </summary>
    public class AnexoController : ControllerBase
    {
        private readonly IAnexoService _anexoService;
        private readonly IFileStorageService _fileStorageService;

        public AnexoController(IAnexoService anexoService, IFileStorageService fileStorageService   )
        {
            _anexoService = anexoService;
            _fileStorageService = fileStorageService;
        }



        
        // CONSULTAS

        [HttpGet("{id}", Name = "GetAnexoPorId")]
        [Permissao(
             "ABRIR_CHAMADO",
             "ASSUMIR_CHAMADO",
             "VISUALIZAR_TODOS_CHAMADOS"
         )]
        [ProducesResponseType(200, Type = typeof(AnexoResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<AnexoResponseDto>>BuscarPorId(int id)
        {

            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Anexo inválido.");

            var anexo = await _anexoService.BuscarPorId(id, usuarioLogadoId, podeVisualizar);

            if (anexo == null)
                return NotFound("Anexo não encontrado.");

            return Ok(anexo);
        }

        [HttpGet("comentario/{comentarioId}")]
        [Permissao(
             "ABRIR_CHAMADO",
             "ASSUMIR_CHAMADO",
             "VISUALIZAR_TODOS_CHAMADOS"
         )]
        [ProducesResponseType(200, Type = typeof(IEnumerable<AnexoResponseDto>))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<AnexoResponseDto>>> BuscarPorComentario(int comentarioId)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");


            if (comentarioId <= 0)
                return BadRequest("Comentário inválido.");

            var anexos =
                await _anexoService.BuscarPorComentario(comentarioId, usuarioLogadoId, podeVisualizar);

            return Ok(anexos);
        }

        [HttpGet("download/{id}")]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Download(int id)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Anexo inválido.");

            var anexo = await _anexoService.BuscarPorId(id, usuarioLogadoId, podeVisualizar);

            if (anexo == null)
                return NotFound("Anexo não encontrado.");

            var arquivo = await _fileStorageService.BaixarArquivo(
                anexo.CaminhoArquivo
            );

            return File(
                arquivo.Arquivo,
                arquivo.ContentType,
                arquivo.Nome
            );
        }

        // AÇÕES

        [Permissao("VISUALIZAR_TODOS_CHAMADOS")]
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Remover(int id)
        {
            var usuarioLogadoId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim(
                "Permissao",
                "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Id inválido.");

            await _anexoService.Remover(
                id,
                usuarioLogadoId,
                podeVisualizar);

            return Ok();
        }

        
    }
}
