using Application.Dto.Comentario;
using Application.Dto.NotificacaoDto;
using Application.Dto.Paginacao;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace Application.Services
{
    // Coordena comentários, permissões de acesso, transições automáticas do
    // chamado, auditoria, notificações e atualizações em tempo real.
    public class ComentarioService : IComentarioService
    {
        private readonly IMapper _mapper;
        private readonly IComentarioRepository _comentarioRepository;
        private readonly IChamadoRepository _chamadoRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly IusuarioService _usuarioService;
        private readonly IPerfilService _perfilService;
        private readonly IAtualizacaoTempoRealService _atualizacaoTempoReal;
        private readonly ITransacaoService _transacaoService;
        private readonly IAnexoRepository _anexoRepository;
        private readonly IFileStorageService _fileStorageService;

        public ComentarioService(
            IMapper mapper,
            IComentarioRepository comentarioRepository,
            IChamadoRepository chamadoRepository,
            INotificacaoService notificacaoService,
            IusuarioService usuarioService,
            IAtualizacaoTempoRealService atualizacaoTempoReal,
            IPerfilService perfilService,
            ITransacaoService transacaoService,
            IAnexoRepository anexoRepository,
            IFileStorageService fileStorageService)
        {
            _mapper = mapper;
            _comentarioRepository = comentarioRepository;
            _chamadoRepository = chamadoRepository;
            _notificacaoService = notificacaoService;
            _usuarioService = usuarioService;
            _atualizacaoTempoReal = atualizacaoTempoReal;
            _perfilService = perfilService;
            _transacaoService = transacaoService;
            _anexoRepository = anexoRepository;
            _fileStorageService = fileStorageService;
        }

        // CRIAÇÃO

        // Cria o comentário e aplica a transição automática correspondente ao
        // autor e ao status atual do chamado antes de persistir a operação.
        public async Task<ComentarioResponseDto> Criar(
            ComentarioRequestDto dto,
            string usuarioLogadoId,
            bool podeVisualizar,
            bool agendarAtualizacaoTempoReal = true)
        {
            var chamado = await _chamadoRepository.BuscarPorId(
                dto.ChamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            var podeParticipar =
                chamado.SolicitanteId == usuarioLogadoId ||
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!podeParticipar && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui permissão!");
            }

            var usuario =
                await _usuarioService.BuscarPorIdInterno(
                    usuarioLogadoId);

            if (usuario == null)
            {
                throw new DomainExceptionValidation(
                    "Usuario não encontrado!");
            }

            var ehSolicitante =
                chamado.SolicitanteId == usuarioLogadoId;

            var statusAnterior = chamado.Status;

            var perfil = await ObterNomePerfil(
                usuario.Id_Perfil);

            var arquivosSalvos = await SalvarArquivos(dto.Arquivos);

            Comentario criar = null!;

            ComentarioResponseDto comentarioDto = null!;

            try
            {
                await _transacaoService.ExecutarAsync(async () =>
                {

                    var statusAlterado = false;

                    if (ehSolicitante)
                    {
                        if (chamado.Status ==
                            StatusChamadoEnum.AguardandoRetorno)
                        {
                            chamado.RetomarAtendimento();
                            statusAlterado = true;
                        }
                    }
                    else
                    {
                        if (chamado.Status ==
                            StatusChamadoEnum.EmAtendimento)
                        {
                            chamado.AguardarRetorno();
                            statusAlterado = true;
                        }
                        else if (chamado.Status ==
                                 StatusChamadoEnum.AguardandoEquipamento)
                        {
                            chamado.ReceberEquipamento();
                            chamado.AguardarRetorno();
                            statusAlterado = true;
                        }
                    }

                    var comentario = new Comentario(
                        dto.ChamadoId,
                        usuarioLogadoId,
                        dto.Mensagem);

                    chamado.RegistrarAuditoria(
                        EventoAuditoriaChamadoEnum.ComentarioCriado,
                        usuario.Id,
                        usuario.Username,
                        perfil,
                        statusAnterior,
                        chamado.Status,
                        "Comentário adicionado ao chamado.",
                        null,
                        JsonSerializer.Serialize(new
                        {
                            comentario.Mensagem
                        }));

                    criar = await _comentarioRepository
                        .Adicionar(comentario);

                    await AdicionarAnexos(
                            criar,
                            chamado,
                            usuario.Id,
                            usuario.Username,
                            perfil,
                            arquivosSalvos);

                    comentarioDto = _mapper.Map<ComentarioResponseDto>(criar);  

                    comentarioDto.UsuarioNome = usuario.Username;


                    if (ehSolicitante)
                    {
                        if (!string.IsNullOrWhiteSpace(
                                chamado.TecnicoResponsavelId))
                        {
                            await _notificacaoService.Adicionar(
                                new NotificacaoRequestDto
                                {
                                    usuarioId =
                                        chamado.TecnicoResponsavelId,
                                    usuarioOrigemId =
                                        usuarioLogadoId,
                                    chamadoId =
                                        chamado.Id_Chamado,
                                    tipo =
                                        TipoNotificacaoEnum.NovoComentario,
                                    titulo = "Novo comentario",
                                    mensagem =
                                        $"{usuario.Username} comentou no chamado {chamado.Numero}.",
                                    status =
                                        StatusNotificacaoEnum.Pendente
                                },
                                agendarAtualizacaoTempoReal);
                        }
                    }
                    else
                    {
                        await _notificacaoService.Adicionar(
                            new NotificacaoRequestDto
                            {
                                usuarioId =
                                    chamado.SolicitanteId,
                                usuarioOrigemId =
                                    usuarioLogadoId,
                                chamadoId =
                                    chamado.Id_Chamado,
                                tipo =
                                    TipoNotificacaoEnum.NovoComentario,
                                titulo = "Novo comentario",
                                mensagem =
                                    $"O {usuario.Username} comentou no chamado {chamado.Numero}. ",
                                status =
                                    StatusNotificacaoEnum.Pendente
                            },
                            agendarAtualizacaoTempoReal);
                    }

                    if (!agendarAtualizacaoTempoReal)
                        return;

                    await _atualizacaoTempoReal
                        .NotificarComentarioCriadoAsync(
                            chamado,
                            usuarioLogadoId,
                            comentarioDto);

                    if (statusAlterado)
                    {
                        var usuariosAfetados = new[]
                        {
                chamado.SolicitanteId ?? string.Empty,
                chamado.TecnicoResponsavelId ?? string.Empty
                };

                        await _atualizacaoTempoReal
                            .NotificarChamadoAtualizadoAsync(
                                usuariosAfetados,
                                chamado.Id_Chamado);

                        await _atualizacaoTempoReal
                            .NotificarDashboardAtualizadoAsync(
                                usuariosAfetados);

                        await _atualizacaoTempoReal
                            .NotificarDashboardGlobalAtualizadoAsync();
                    }
                });
            }
            catch
            {
                await RemoverArquivos(arquivosSalvos);
                throw;
            }
           
            return comentarioDto;
        }

        // ALTERAÇÃO

        // Permite a edição pelo autor ou por quem possui acesso administrativo
        // e registra os valores anterior e atual na auditoria do chamado.
        public async Task<ComentarioResponseDto> Atualizar(
            int id,
            ComentarioRequestDto dto,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var comentario = await _comentarioRepository.BuscarPorId(id);

            if (comentario == null)
                throw new DomainExceptionValidation(
                    "Comentário não encontrado.");

            var podeParticipar =
                comentario.UsuarioId == usuarioLogadoId;

            if (!podeParticipar && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui permissão!");
            }

            var chamado = await _chamadoRepository
                .BuscarPorId(comentario.ChamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            var usuario = await _usuarioService
                .BuscarPorIdInterno(usuarioLogadoId);

            var perfil = await ObterNomePerfil(usuario.Id_Perfil);
            var mensagemAnterior = comentario.Mensagem;

            comentario.AtualizarMensagem(dto.Mensagem);

            chamado.RegistrarAuditoria(
                EventoAuditoriaChamadoEnum.ComentarioEditado,
                usuario.Id,
                usuario.Username,
                perfil,
                chamado.Status,
                chamado.Status,
                "Comentário do chamado editado.",
                JsonSerializer.Serialize(new
                {
                    ComentarioId = comentario.Id_Comentario,
                    Mensagem = mensagemAnterior
                }),
                JsonSerializer.Serialize(new
                {
                    ComentarioId = comentario.Id_Comentario,
                    comentario.Mensagem
                }));

            var atualizar =
                await _comentarioRepository.Atualizar(comentario);

            return await MapearComentarioComAutor(atualizar);
        }

        // CONSULTAS

        public async Task<ComentarioResponseDto?> BuscarPorId(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var comentario = await _comentarioRepository.BuscarPorId(id);

            if (comentario == null)
                throw new DomainExceptionValidation(
                    "Comentario não encontrado!");

            var chamado =
                await _chamadoRepository.BuscarPorId(comentario.ChamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            var podeParticipar =
                chamado.SolicitanteId == usuarioLogadoId ||
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!podeParticipar && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui permissão!");
            }

            return await MapearComentarioComAutor(comentario);
        }

        public async Task<IEnumerable<ComentarioResponseDto>>
            BuscarPorChamado(
                int chamadoId,
                string usuarioLogadoId,
                bool podeVisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            var podeParticipar =
                chamado.SolicitanteId == usuarioLogadoId ||
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!podeParticipar && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui permissão!");
            }

            var comentarios =
                await _comentarioRepository.BuscarPorChamado(chamadoId);

            return await MapearComentariosComAutores(comentarios);
        }

        // REMOÇÃO

        // Registra uma fotografia do comentário antes de removê-lo.
        public async Task Remover(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var comentario = await _comentarioRepository.BuscarPorId(id);

            if (comentario == null)
                throw new DomainExceptionValidation(
                    "Comentário não encontrado.");

            var chamado = await _chamadoRepository
                .BuscarPorId(comentario.ChamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            var ehAutor = comentario.UsuarioId == usuarioLogadoId;

            if (!ehAutor && !podeVisualizar)
                throw new AcessoNegadoException(
                    "Você não possui permissão!");

            var usuario = await _usuarioService
                .BuscarPorIdInterno(usuarioLogadoId);

            var perfil = await ObterNomePerfil(usuario.Id_Perfil);

            chamado.RegistrarAuditoria(
                EventoAuditoriaChamadoEnum.ComentarioRemovido,
                usuario.Id,
                usuario.Username,
                perfil,
                chamado.Status,
                chamado.Status,
                "Comentário removido do chamado.",
                JsonSerializer.Serialize(new
                {
                    ComentarioId = comentario.Id_Comentario,
                    comentario.UsuarioId,
                    comentario.Mensagem,
                    comentario.DataCriacao
                }),
                null);

            await _comentarioRepository.Remover(id);
        }

        // PAGINAÇÃO

        public async Task<PaginacaoResponseDto<ComentarioResponseDto>>
            BuscarPorChamadoPaginado(
                int chamadoId,
                PaginacaoRequestDto request,
                string usuarioLogadoId,
                bool podeVisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado!");

            var podeParticipar =
                chamado.SolicitanteId == usuarioLogadoId ||
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!podeParticipar && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui permissão!");
            }

            var resultado =
                await _comentarioRepository.BuscarPorChamadoPaginacao(
                    chamadoId,
                    request.Pagina,
                    request.TamanhoPagina);

            var comentariosDto =
                await MapearComentariosComAutores(resultado.Comentario);

            return new PaginacaoResponseDto<ComentarioResponseDto>
            {
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.TotalRegsitro,
                TotalPaginas = (int)Math.Ceiling(
                    resultado.TotalRegsitro /
                    (double)request.TamanhoPagina),
                Dados = comentariosDto
            };
        }

        // MÉTODOS INTERNOS

        // Complementa o DTO com o nome do autor, que pertence ao Identity.
        private async Task<ComentarioResponseDto> MapearComentarioComAutor(
            Comentario comentario)
        {
            var comentarioDto =
                _mapper.Map<ComentarioResponseDto>(comentario);

            var usuario = await _usuarioService
                .BuscarPorIdInterno(comentario.UsuarioId);

            comentarioDto.UsuarioNome =
                usuario?.Username ?? "Usuário";

            return comentarioDto;
        }

        // Evita buscar repetidamente o mesmo usuário quando ele possui mais
        // de um comentário na coleção recebida.
        private async Task<IEnumerable<ComentarioResponseDto>>
            MapearComentariosComAutores(
                IEnumerable<Comentario> comentarios)
        {
            var comentariosDto =
                _mapper.Map<List<ComentarioResponseDto>>(comentarios);

            var nomesPorUsuario = new Dictionary<string, string>();

            foreach (var usuarioId in comentariosDto
                .Select(x => x.UsuarioId)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct())
            {
                var usuario =
                    await _usuarioService.BuscarPorIdInterno(usuarioId);

                nomesPorUsuario[usuarioId] =
                    usuario?.Username ?? "Usuário";
            }

            foreach (var comentario in comentariosDto)
            {
                comentario.UsuarioNome = nomesPorUsuario.GetValueOrDefault(
                    comentario.UsuarioId,
                    "Usuário");
            }

            return comentariosDto;
        }

        // Resolve o nome do perfil para armazená-lo junto à auditoria.
        private async Task<string?> ObterNomePerfil(int? perfilId)
        {
            if (!perfilId.HasValue)
                return null;

            var perfil = await _perfilService.BuscarPorId(perfilId.Value);

            return perfil?.Nome;
        }

        // Salva os arquivos físicos antes da transação e mantém seus metadados
        // para a criação dos anexos. Se algum upload falhar, remove todos os
        // arquivos já salvos e preserva a exceção original.
        private async Task<List<(string NomeArquivo, string NomeOriginal,string ContentType,
            long Tamanho,string CaminhoArquivo)>> SalvarArquivos(IEnumerable<IFormFile> arquivos)
        {
            var arquivosSalvos = new List<(
                string NomeArquivo,
                string NomeOriginal,
                string ContentType,
                long Tamanho,
                string CaminhoArquivo)>();

            try
            {
                foreach (var arquivo in arquivos)
                {
                    var caminhoArquivo =
                        await _fileStorageService.SalvarArquivo(arquivo);

                    arquivosSalvos.Add((
                        Path.GetFileName(caminhoArquivo),
                        arquivo.FileName,
                        arquivo.ContentType,
                        arquivo.Length,
                        caminhoArquivo));
                }

                return arquivosSalvos;
            }
            catch
            {
                foreach (var arquivoSalvo in arquivosSalvos)
                {
                    try
                    {
                        await _fileStorageService.RemoverArquivo(
                            arquivoSalvo.CaminhoArquivo);
                    }
                    catch
                    {
                        // Preserva o erro original do upload.
                    }
                }

                throw;
            }
        }

        // Cria os registros dos anexos vinculados ao comentário e registra
        // uma auditoria para cada arquivo dentro da mesma transação.
        private async Task AdicionarAnexos(
            Comentario comentario,
            Chamado chamado,
            string usuarioId,
            string usuarioNome,
            string? perfilUsuario,
            IEnumerable<(
                string NomeArquivo,
                string NomeOriginal,
                string ContentType,
                long Tamanho,
                string CaminhoArquivo)> arquivosSalvos)
        {
            foreach (var arquivo in arquivosSalvos)
            {
                var anexo = new Anexo(
                    comentario.Id_Comentario,
                    arquivo.NomeArquivo,
                    arquivo.NomeOriginal,
                    arquivo.ContentType,
                    arquivo.Tamanho,
                    arquivo.CaminhoArquivo);

                chamado.RegistrarAuditoria(
                    EventoAuditoriaChamadoEnum.AnexoAdicionado,
                    usuarioId,
                    usuarioNome,
                    perfilUsuario,
                    chamado.Status,
                    chamado.Status,
                    "Anexo adicionado a um comentário do chamado.",
                    null,
                    JsonSerializer.Serialize(new
                    {
                        anexo.ComentarioId,
                        anexo.NomeOriginal,
                        anexo.ContentType,
                        anexo.Tamanho
                    }));

                await _anexoRepository.Adicionar(anexo);
            }
        }

        // Remove os arquivos físicos preparados quando o banco não consegue
        // concluir a operação, preservando a exceção que causou a falha.
        private async Task RemoverArquivos(
            IEnumerable<(
                string NomeArquivo,
                string NomeOriginal,
                string ContentType,
                long Tamanho,
                string CaminhoArquivo)> arquivosSalvos)
        {
            foreach (var arquivo in arquivosSalvos)
            {
                try
                {
                    await _fileStorageService.RemoverArquivo(
                        arquivo.CaminhoArquivo);
                }
                catch
                {
                    // A falha da limpeza não substitui o erro original.
                }
            }
        }


    }
}
