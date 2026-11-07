using Application.Dto.CategoriaAtendimentoDto;
using Application.Interfaces;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Gerencia as categorias utilizadas na classificação dos chamados.
    /// </summary>
    public class CategoriaAtendimentoController : ControllerBase
    {
        private readonly ICategoriaAtendimentoService _categoriaService;

        public CategoriaAtendimentoController(
            ICategoriaAtendimentoService categoriaService)
        {
            _categoriaService = categoriaService;
        }


        // CONSULTAS

        [HttpGet("{id}", Name = "GetCategoriaAtendimentoPorId")]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS",
            "GERENCIAR_CATEGORIAS",
            "GERENCIAR_DISTRIBUICAO",
            "VISUALIZAR_CATEGORIAS"
        )]
        [ProducesResponseType(200, Type = typeof(CategoriaAtendimentoResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CategoriaAtendimentoResponseDto>>
            BuscarPorId(int id)
        {
            if (id <= 0)
                return BadRequest("Categoria inválida.");

            var categoria =
                await _categoriaService.BuscarPorId(id);

            if (categoria == null)
                return NotFound("Categoria não encontrada.");

            return Ok(categoria);
        }

        [HttpGet]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS",
            "GERENCIAR_CATEGORIAS",
            "GERENCIAR_DISTRIBUICAO",
            "VISUALIZAR_CATEGORIAS"
        )]
        [ProducesResponseType(200, Type = typeof(IEnumerable<CategoriaAtendimentoResponseDto>))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<CategoriaAtendimentoResponseDto>>>
            BuscarTodos()
        {
            var categorias =
                await _categoriaService.BuscarTodos();

            if (!categorias.Any())
                return NotFound("Nenhuma categoria encontrada.");

            return Ok(categorias);
        }

        // AÇÕES

        [Permissao("GERENCIAR_CATEGORIAS")]
        [HttpPost]
        [ProducesResponseType(200, Type = typeof(CategoriaAtendimentoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult>
            Criar([FromBody] CategoriaAtendimentoRequestDto dto)
        {
            var categoria =
                await _categoriaService.Criar(dto);

            return Ok(categoria);
        }

        [Permissao("GERENCIAR_CATEGORIAS")]
        [HttpPut("{id}")]
        [ProducesResponseType(200, Type = typeof(CategoriaAtendimentoResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Atualizar(int id, [FromBody] CategoriaAtendimentoRequestDto dto)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            var categoria =
                await _categoriaService.Atualizar(id, dto);

            return Ok(categoria);
        }

        [Permissao("GERENCIAR_CATEGORIAS")]
        [HttpPut("{id}/ativar")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Ativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _categoriaService.Ativar(id);

            return Ok("Categoria ativada com sucesso.");
        }

        [Permissao("GERENCIAR_CATEGORIAS")]
        [HttpPut("{id}/desativar")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult>
            Desativar(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _categoriaService.Desativar(id);

            return Ok("Categoria desativada com sucesso.");
        }
        [Permissao("GERENCIAR_CATEGORIAS")]
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult>
            Remover(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _categoriaService.Remover(id);

            return Ok();
        }
    }
}
