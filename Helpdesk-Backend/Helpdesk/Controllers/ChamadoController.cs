using Application.Dto.Chamado;
using Application.Dto.Paginacao;
using Application.Interfaces;
using Application.ViewModel;
using Helpdesk.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
namespace Helpdesk.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    /// <summary>
    /// Centraliza consultas e operações do ciclo de vida dos chamados.
    /// </summary>
    public class ChamadoController : ControllerBase
    {
        private readonly IChamadoService _chamadoService;

        public ChamadoController(IChamadoService chamadoService)
        {
            _chamadoService = chamadoService;
        }

        
        // CONSULTAS

        [HttpGet("{id}", Name = "GetChamadoPorId")]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ChamadoResponseDto>> BuscarPorId(int id)
        {
            var usuarioId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioId))
                return Unauthorized();

            var podeVisualizarTodos = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Chamado inválido.");

            var chamado = await _chamadoService.BuscarPorId(id, usuarioId, podeVisualizarTodos);

            if (chamado == null)
                return NotFound("Chamado não encontrado.");

            return Ok(chamado);
        }

        
        
        [HttpGet]
        [Permissao("VISUALIZAR_TODOS_CHAMADOS")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChamadoResponseDto>))]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<ChamadoResponseDto>>> BuscarTodos()
        {
            var chamados = await _chamadoService.BuscarTodos();

            if (!chamados.Any())
                return NotFound("Nenhum chamado encontrado.");

            return Ok(chamados);
        }

        
        [HttpGet("fila")]
        [Permissao("VISUALIZAR_FILA")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChamadoResponseDto>))]
        public async Task<ActionResult<IEnumerable<ChamadoResponseDto>>> BuscarFila()
        {
            var chamados =
                await _chamadoService.BuscarFilaAtendimento();

            return Ok(chamados);
        }

        [HttpGet("meus-chamados/{tecnicoId}")]
        [Permissao("ASSUMIR_CHAMADO", "VISUALIZAR_TODOS_CHAMADOS")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChamadoResponseDto>))]
        public async Task<ActionResult<IEnumerable<ChamadoResponseDto>>>
            BuscarMeusChamados(string tecnicoId)
        {
            if (!PossuiPermissao("VISUALIZAR_TODOS_CHAMADOS"))
            {
                tecnicoId = ObterUsuarioAutenticado();
                if (string.IsNullOrWhiteSpace(tecnicoId))
                    return Unauthorized();
            }

            var chamados =
                await _chamadoService.BuscarMeusChamados(tecnicoId);

            return Ok(chamados);
        }


        [HttpGet("meus-chamados-solicitante/{solicitanteId}")]
        [Permissao("ABRIR_CHAMADO", "VISUALIZAR_TODOS_CHAMADOS")]
        [ProducesResponseType(200, Type = typeof(IEnumerable<ChamadoResponseDto>))]
        public async Task<ActionResult<IEnumerable<ChamadoResponseDto>>>
            BuscarMeusChamadosSolicitante(string solicitanteId)
        {
            if (!PossuiPermissao("VISUALIZAR_TODOS_CHAMADOS"))
            {
                solicitanteId = ObterUsuarioAutenticado();
                if (string.IsNullOrWhiteSpace(solicitanteId))
                    return Unauthorized();
            }

            var chamados =
                await _chamadoService.BuscarMeusChamadosSolicitante(solicitanteId);

            return Ok(chamados);
        }


        [HttpGet("historico-paginado")]
        [Permissao(
            "VISUALIZAR_HISTORICO",
            "EXPORTAR_HISTORICO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(PaginacaoResponseDto<ChamadoResponseDto>))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaginacaoResponseDto<ChamadoResponseDto>>>
        BuscarHistoricoPaginado(
        [FromQuery] DateTime? dataInicial,
        [FromQuery] DateTime? dataFinal,
        [FromQuery] PaginacaoRequestDto request)
        {
            var usuarioId  =
               User.FindFirstValue(
                   ClaimTypes.NameIdentifier
               );

            var perfilId = int.Parse(User.FindFirst("PerfilId")!.Value);

            var resultado =
                await _chamadoService.BuscarHistoricoPaginado(
                    usuarioId,
                    perfilId,
                    dataInicial,
                    dataFinal,
                    request);

            return Ok(resultado);
        }

        [HttpGet("analise-paginada")]
        [Permissao("VISUALIZAR_AUDITORIA_CHAMADO")]
        [ProducesResponseType(
            StatusCodes.Status200OK,
            Type = typeof(PaginacaoResponseDto<ChamadoResponseDto>))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<
            PaginacaoResponseDto<ChamadoResponseDto>>>
            BuscarAnalisePaginada(
                [FromQuery] AnaliseChamadoFiltroViewModel filtro,
                [FromQuery] PaginacaoRequestDto request)
        {
            var resultado = await _chamadoService
                .BuscarAnalisePaginada(filtro, request);

            return Ok(resultado);
        }

        [HttpGet("fila-paginado")]
        [Permissao("VISUALIZAR_FILA")]
        [ProducesResponseType(200, Type = typeof(PaginacaoResponseDto<ChamadoResponseDto>))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaginacaoResponseDto<ChamadoResponseDto>>> BuscarFilaAtendimentoPaginado(
            [FromQuery] DateTime? dataInicial,
            [FromQuery] DateTime? dataFinal, 
            [FromQuery] int prioridade, 
            [FromQuery] PaginacaoRequestDto request)
            {
            var resultado =
                await _chamadoService.BuscarFilaAtendimentoPaginado( prioridade, request,dataInicial, dataFinal);

            return Ok(resultado);
        }

        [HttpGet("meus-chamados-paginado/{tecnicoId}")]
        [Permissao(
             "ABRIR_CHAMADO",
            "VISUALIZAR_HISTORICO",
            "EXPORTAR_HISTORICO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(PaginacaoResponseDto<ChamadoResponseDto>))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaginacaoResponseDto<ChamadoResponseDto>>>
        BuscarMeusChamadosPaginado(
        string tecnicoId,
        [FromQuery] PaginacaoRequestDto request)
        {
            var visualizarTodos = PossuiPermissao("VISUALIZAR_TODOS_CHAMADOS");

            if (!visualizarTodos)
            {
                tecnicoId = ObterUsuarioAutenticado();
                if (string.IsNullOrWhiteSpace(tecnicoId))
                    return Unauthorized();
            }

            var resultado =
                await _chamadoService.BuscarMeusChamadosPaginado(visualizarTodos,
                    tecnicoId,
                    request);

            return Ok(resultado);
        }

        [HttpGet("meus-chamados-solicitante-paginado/{solicitanteId}")]
        [Permissao(
             "ABRIR_CHAMADO",
            "VISUALIZAR_HISTORICO",
            "EXPORTAR_HISTORICO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(PaginacaoResponseDto<ChamadoResponseDto>))]
        [ProducesResponseType(400)]
        public async Task<ActionResult<PaginacaoResponseDto<ChamadoResponseDto>>>
        BuscarMeusChamadosSolicitantePaginado(
        string solicitanteId,
        [FromQuery] PaginacaoRequestDto request)
        {
            var visualizarTodos = PossuiPermissao("VISUALIZAR_TODOS_CHAMADOS");

            if (!visualizarTodos)
            {
                solicitanteId = ObterUsuarioAutenticado();
                if (string.IsNullOrWhiteSpace(solicitanteId))
                    return Unauthorized();
            }

            var resultado =
                await _chamadoService.BuscarMeusChamadosSolicitantePaginado(
                    visualizarTodos,
                    solicitanteId,
                    request);

            return Ok(resultado);
        }

        // AÇÕES

        // Cadastro e manutenção

        [HttpPost]
        [Permissao("ABRIR_CHAMADO")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Criar(
            [FromBody] ChamadoRequestDto dto)
        {
           
            var solicitanteId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier
                );
            var chamado =
                await _chamadoService.Criar(
                    solicitanteId,
                    dto);

            return Ok(chamado);
        }

        [HttpPut("{id}")]
        [Permissao(
            "ABRIR_CHAMADO",
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Atualizar(
            int id,
            [FromBody] ChamadoRequestDto dto)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamado =
                await _chamadoService.Atualizar(id, dto, usuarioLogadoId, podeVisualizar);

            return Ok(chamado);
        }

        // Atendimento

        [Permissao("ASSUMIR_CHAMADO")]
        [HttpPut("{id}/assumir")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> AssumirAtendimento(int id,[FromQuery] string? tecnicoId = null)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            tecnicoId = ObterUsuarioAutenticado();
            if (string.IsNullOrWhiteSpace(tecnicoId))
                return Unauthorized();

            var chamado =
                await _chamadoService.AssumirAtendimento(
                    id,
                    tecnicoId);

            return Ok(chamado);
        }
        [Permissao("ASSUMIR_CHAMADO")]
        [HttpPut("{id}/aguardar-retorno")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> AguardarRetorno(int id)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");


            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamado =
                await _chamadoService.AguardarRetorno(id, usuarioLogadoId, podeVisualizar);

            return Ok(chamado);
        }

        [Permissao("ASSUMIR_CHAMADO",
                    "ABRIR_CHAMADO",
                    "VISUALIZAR_TODOS_CHAMADOS")]
        [HttpPut("{id}/retomar-atendimento")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> RetomarAtendimento(int id)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");
            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamado =
                await _chamadoService.RetomarAtendimento(id,usuarioLogadoId,podeVisualizar);

            return Ok(chamado);
        }

        // Equipamento

        [Permissao("ABRIR_CHAMADO")]
        [HttpPut("{id}/enviar-equipamento")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> EnviarEquipamento(int id)
        {
            var usuarioLogadoId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamado =
                await _chamadoService.EnviarEquipamento(
                    id,
                    usuarioLogadoId);

            return Ok(chamado);
        }

        [Permissao(
            "ASSUMIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [HttpPut("{id}/receber-equipamento")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> ReceberEquipamento(int id)
        {
            var usuarioLogadoId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            if (id <= 0)
                return BadRequest("Id inválido.");

            var podeVisualizar =
                User.HasClaim(
                    "Permissao",
                    "VISUALIZAR_TODOS_CHAMADOS");

            var chamado =
                await _chamadoService.ReceberEquipamento(
                    id,
                    usuarioLogadoId,
                    podeVisualizar);

            return Ok(chamado);
        }

        // Transferência

        [Permissao("ASSUMIR_CHAMADO")]
        [HttpPut("{id}/transferir")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> TransferirChamado( int id, [FromQuery] string destinoId)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");


            if (id <= 0)
                return BadRequest("Id inválido.");

            if (string.IsNullOrWhiteSpace(destinoId))
                return BadRequest("Técnico de destino inválido.");

            var chamado =
                await _chamadoService.TransferirChamado(
                    id,
                    destinoId, usuarioLogadoId, podeVisualizar);

            return Ok(chamado);
        }

        [Permissao("ASSUMIR_CHAMADO")]
        [HttpPut("{id}/aceitar-transferencia")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> AceitarTransferencia(int id,[FromQuery] int notificacaoId)
        {

            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (notificacaoId <= 0)
                return BadRequest("Notificação inválida.");

            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamado =
                await _chamadoService.AceitarChamado(
                    id,
                    notificacaoId,
                    usuarioLogadoId,
                    podeVisualizar);

            return Ok(chamado);
        }

        [Permissao("ASSUMIR_CHAMADO")]
        [HttpPut("recusar-transferencia")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> RecusarTransferencia([FromQuery] int notificacaoId)
        {
            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");



            if (notificacaoId <= 0)
                return BadRequest("Notificação inválida.");

            await _chamadoService.RecusarChamado(notificacaoId, usuarioLogadoId, podeVisualizar);

            return Ok();
        }


        // Encerramento

        [Permissao("ENCERRAR_CHAMADO")]
        [HttpPut("{id}/fechar")]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> FecharChamado(
            int id,
            [FromBody] FecharChamadoDto dto)
        {

            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamado =
                await _chamadoService.FecharChamado(
                    id,
                    dto,
                    usuarioLogadoId,
                    podeVisualizar);

            return Ok(chamado);
        }

       
        [HttpPut("{id}/cancelar")]
        [Permissao(
            "ABRIR_CHAMADO",
            "VISUALIZAR_TODOS_CHAMADOS"
        )]
        [ProducesResponseType(200, Type = typeof(ChamadoResponseDto))]
        [ProducesResponseType(400)]
        public async Task<ActionResult> CancelarChamado(int id)
        {

            var usuarioLogadoId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(usuarioLogadoId))
                return Unauthorized();

            var podeVisualizar = User.HasClaim("Permissao", "VISUALIZAR_TODOS_CHAMADOS");

            if (id <= 0)
                return BadRequest("Id inválido.");

            var chamado =
                await _chamadoService.CancelarChamado(id, usuarioLogadoId, podeVisualizar);

            return Ok(chamado);
        }

        [Permissao("VISUALIZAR_TODOS_CHAMADOS")]
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult> Remover(int id)
        {
            if (id <= 0)
                return BadRequest("Id inválido.");

            await _chamadoService.Remover(id);

            return Ok();
        }

        // MÉTODOS PRIVADOS

        private string? ObterUsuarioAutenticado() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private bool PossuiPermissao(string permissao) =>
            User.HasClaim("Permissao", permissao);

    }
}
