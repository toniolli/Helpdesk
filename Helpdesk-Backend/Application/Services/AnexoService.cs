
using Application.DTOs.Anexo;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;
using System.Text.Json;

namespace Application.Services
{
    // Coordena autorização, armazenamento físico, persistência e auditoria dos anexos.
    // A entidade Anexo armazena apenas os metadados; o arquivo é tratado por IFileStorageService.
    public class AnexoService : IAnexoService
    {
        private readonly IMapper _mapper;
        private readonly IAnexoRepository _anexoRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IComentarioRepository _comentarioRepository;
        private readonly IChamadoRepository _chamadoRepository;
        private readonly IusuarioService _usuarioService;
        private readonly IPerfilService _perfilService;

        public AnexoService(
            IMapper mapper,
            IAnexoRepository anexoRepository,
            IFileStorageService fileStorageService,
            IComentarioRepository comentarioRepository,
            IChamadoRepository chamadoRepository,
            IusuarioService usuarioService,
            IPerfilService perfilService)
        {
            _mapper = mapper;
            _anexoRepository = anexoRepository;
            _fileStorageService = fileStorageService;
            _comentarioRepository = comentarioRepository;
            _chamadoRepository = chamadoRepository;
            _usuarioService = usuarioService;
            _perfilService = perfilService;
        }

       

        // ==========================
        // CONSULTAS
        // ==========================

        public async Task<AnexoResponseDto?> BuscarPorId(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var anexo = await _anexoRepository.BuscarPorId(id);

            if (anexo == null)
                return null;

            await ValidarAcessoAoComentario(
                anexo.ComentarioId,
                usuarioLogadoId,
                podeVisualizar);

            return _mapper.Map<AnexoResponseDto>(anexo);
        }

        public async Task<IEnumerable<AnexoResponseDto>> BuscarPorComentario(
            int comentarioId,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            await ValidarAcessoAoComentario(
                comentarioId,
                usuarioLogadoId,
                podeVisualizar);

            var anexos = await _anexoRepository.BuscarPorComentario(comentarioId);

            if (anexos == null)
                return Enumerable.Empty<AnexoResponseDto>();

            return _mapper.Map<IEnumerable<AnexoResponseDto>>(anexos);
        }

        // ==========================
        // REMOÇÃO
        // ==========================

        public async Task Remover(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var anexo = await _anexoRepository.BuscarPorId(id);

            if (anexo == null)
                throw new DomainExceptionValidation("anexo não encontrado.");

            await ValidarAcessoAoComentario(
                anexo.ComentarioId,
                usuarioLogadoId,
                podeVisualizar);

            // O chamado rastreado recebe a auditoria antes da remoção dos metadados.
            // O SaveChanges do repositório confirma as duas alterações no mesmo contexto.
            var comentario = await _comentarioRepository
                .BuscarPorId(anexo.ComentarioId);
            var chamado = await _chamadoRepository
                .BuscarPorId(comentario!.ChamadoId);
            var usuario = await _usuarioService
                .BuscarPorIdInterno(usuarioLogadoId);
            var perfil = await ObterNomePerfil(usuario.Id_Perfil);

            chamado!.RegistrarAuditoria(
                EventoAuditoriaChamadoEnum.AnexoRemovido,
                usuario.Id,
                usuario.Username,
                perfil,
                chamado.Status,
                chamado.Status,
                "Anexo removido de um comentário do chamado.",
                JsonSerializer.Serialize(new
                {
                    AnexoId = anexo.Id_Anexo,
                    anexo.ComentarioId,
                    anexo.NomeOriginal,
                    anexo.ContentType,
                    anexo.Tamanho
                }),
                null);

            await _anexoRepository.Remover(id);

            // O arquivo físico só é removido depois que o banco confirma a exclusão.
            await _fileStorageService.RemoverArquivo(anexo.CaminhoArquivo);
        }

        // ==========================
        // MÉTODOS INTERNOS
        // ==========================

        // Centraliza a autorização usada por criação, consulta e remoção de anexos.
        private async Task ValidarAcessoAoComentario(
            int comentarioId,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var comentario =
                await _comentarioRepository.BuscarPorId(comentarioId);

            if (comentario == null)
                throw new DomainExceptionValidation(
                    "Comentário não encontrado.");

            var chamado =
                await _chamadoRepository.BuscarPorId(comentario.ChamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation(
                    "Chamado não encontrado.");

            var participaDoChamado =
                chamado.SolicitanteId == usuarioLogadoId ||
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!participaDoChamado && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui acesso a este anexo.");
            }
        }

        // Resolve o nome apresentado na auditoria sem criar valor artificial para usuário sem perfil.
        private async Task<string?> ObterNomePerfil(int? perfilId)
        {
            if (!perfilId.HasValue)
                return null;

            var perfil = await _perfilService.BuscarPorId(perfilId.Value);
            return perfil?.Nome;
        }
    }
}
