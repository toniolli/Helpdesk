using Application.Dto.NotificacaoDto;
using Application.Dto.Paginacao;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena notificações, permissões de acesso, alterações de estado e a
    // atualização da Topbar dos destinatários em tempo real.
    public class NotificacaoService : INotificacaoService
    {
        private readonly IMapper _mapper;
        private readonly INotificacaoRepository _notificacaoRepository;
        private readonly IAtualizacaoTempoRealService _atualizacaoTempoRealService;
        private readonly ITransacaoService _transacaoService;


        public NotificacaoService(
            IMapper mapper,
            INotificacaoRepository notificacaoRepository,
            IAtualizacaoTempoRealService atualizacaoTempoRealService,
            ITransacaoService transacaoService)
        {
            _mapper = mapper;
            _notificacaoRepository = notificacaoRepository;
            _atualizacaoTempoRealService = atualizacaoTempoRealService;
            _transacaoService = transacaoService;
        }

        // CRIAÇÃO

        // Persiste a notificação e avisa somente seu destinatário para que a
        // Topbar recarregue os dados.
        public async Task<NotificacaoResponseDto> Adicionar(
            NotificacaoRequestDto dto, bool agendarAtualizacaoTempoReal = true)
        {
            var notificacao = _mapper.Map<Notificacao>(dto);

            await _transacaoService.ExecutarAsync(async () =>
            {
                await _notificacaoRepository.Adicionar(notificacao);

                if (agendarAtualizacaoTempoReal)
                {
                    await _atualizacaoTempoRealService
                        .NotificarNotificacaoAtualizadaAsync(
                            notificacao.UsuarioId);
                }
            });

            return _mapper.Map<NotificacaoResponseDto>(notificacao);
        }

        // MÉTODOS INTERNOS

        // Operações internas não aplicam a validação do usuário logado porque
        // são utilizadas pelos próprios fluxos controlados da aplicação.
        public async Task<NotificacaoResponseDto> BuscarPorIdInterno(
            int id)
        {
            var notificacao =
                await _notificacaoRepository.BuscarPorId(id);

            return _mapper.Map<NotificacaoResponseDto>(notificacao);
        }

        public async Task MarcarComoLidaInterno(int id)
        {
            var notificacao =
                await BuscarNotificacaoObrigatoria(id);

            notificacao.MarcarComoLida();

            await SalvarAtualizacao(notificacao);
        }

        public async Task AceitarInterno(int id)
        {
            var notificacao =
                await BuscarNotificacaoObrigatoria(id);

            notificacao.Aceitar();

            await SalvarAtualizacao(notificacao);
        }

        public async Task RecusarInterno(int id)
        {
            var notificacao =
                await BuscarNotificacaoObrigatoria(id);

            notificacao.Recusar();

            await SalvarAtualizacao(notificacao);
        }

        public async Task ExpirarInterno(int id)
        {
            var notificacao =
                await BuscarNotificacaoObrigatoria(id);

            notificacao.Expirar();

            await SalvarAtualizacao(notificacao);
        }

        // MÉTODOS PROTEGIDOS

        // As operações protegidas limitam o acesso ao destinatário, exceto
        // quando a permissão recebida autoriza a visualização global.
        public async Task<NotificacaoResponseDto> BuscarPorId(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var notificacao =
                await BuscarNotificacaoEValidarAcesso(
                    id,
                    usuarioLogadoId,
                    podeVisualizar);

            return _mapper.Map<NotificacaoResponseDto>(notificacao);
        }

        public async Task<IEnumerable<NotificacaoResponseDto>> BuscarTodos(
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var notificacoes = podeVisualizar
                ? await _notificacaoRepository.BuscarTodos()
                : await _notificacaoRepository.BuscarTodos(
                    usuarioLogadoId);

            return _mapper.Map<IEnumerable<NotificacaoResponseDto>>(
                notificacoes);
        }

        public async Task<PaginacaoResponseDto<NotificacaoResponseDto>>
            BuscarNotificacaoPaginado(
                PaginacaoRequestDto request,
                string usuarioLogadoId,
                bool podeVisualizar)
        {
            var resultado = podeVisualizar
                ? await _notificacaoRepository
                    .BuscarNotificacaoPaginado(
                        request.Pagina,
                        request.TamanhoPagina)
                : await _notificacaoRepository
                    .BuscarNotificacaoPaginado(
                        request.Pagina,
                        request.TamanhoPagina,
                        usuarioLogadoId);

            return CriarRespostaPaginada(request, resultado);
        }

        public async Task<IEnumerable<NotificacaoResponseDto>>
            ObterPorUsuario(
                string usuarioId,
                string usuarioLogadoId,
                bool podeVisualizar)
        {
            ValidarAcessoAoUsuario(
                usuarioId,
                usuarioLogadoId,
                podeVisualizar);

            var notificacoes =
                await _notificacaoRepository.ObterPorUsuario(
                    usuarioId);

            return _mapper.Map<IEnumerable<NotificacaoResponseDto>>(
                notificacoes);
        }

        public async Task<IEnumerable<NotificacaoResponseDto>>
            ObterNaoLidas(
                bool lida,
                string usuarioLogadoId,
                bool podeVisualizar)
        {
            var notificacoes = podeVisualizar
                ? await _notificacaoRepository.ObterNaoLidas(lida)
                : await _notificacaoRepository.ObterNaoLidas(
                    lida,
                    usuarioLogadoId);

            return _mapper.Map<IEnumerable<NotificacaoResponseDto>>(
                notificacoes);
        }

        public async Task MarcarComoLida(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var notificacao =
                await BuscarNotificacaoEValidarAcesso(
                    id,
                    usuarioLogadoId,
                    podeVisualizar);

            notificacao.MarcarComoLida();

            await SalvarAtualizacao(notificacao);
        }

        public async Task Aceitar(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var notificacao =
                await BuscarNotificacaoEValidarAcesso(
                    id,
                    usuarioLogadoId,
                    podeVisualizar);

            notificacao.Aceitar();

            await SalvarAtualizacao(notificacao);
        }

        public async Task Recusar(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var notificacao =
                await BuscarNotificacaoEValidarAcesso(
                    id,
                    usuarioLogadoId,
                    podeVisualizar);

            notificacao.Recusar();

            await SalvarAtualizacao(notificacao);
        }

        public async Task Expirar(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var notificacao =
                await BuscarNotificacaoEValidarAcesso(
                    id,
                    usuarioLogadoId,
                    podeVisualizar);

            notificacao.Expirar();

            await SalvarAtualizacao(notificacao);
        }

        public async Task Pendente(
            int id,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var notificacao =
                await BuscarNotificacaoEValidarAcesso(
                    id,
                    usuarioLogadoId,
                    podeVisualizar);

            notificacao.Pendente();

            await SalvarAtualizacao(notificacao);
        }

        public async Task<int> TotalNotificacaoNaoLidas(
            string usuarioId,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            ValidarAcessoAoUsuario(
                usuarioId,
                usuarioLogadoId,
                podeVisualizar);

            return await _notificacaoRepository
                .TotalNotificacaoNaoLidas(usuarioId);
        }

        // DISTRIBUIÇÃO

        public async Task<IEnumerable<NotificacaoResponseDto>>
            BuscarPendentesDistribuicaoPerfil(int chamadoId)
        {
            var notificacoes =
                await _notificacaoRepository
                    .BuscarPendentesDistribuicaoPerfil(chamadoId);

            return _mapper.Map<IEnumerable<NotificacaoResponseDto>>(
                notificacoes);
        }

        public async Task<bool>
            ExisteNotificacaoPorChamadoTipoEUsuario(
                int chamadoId,
                TipoNotificacaoEnum tipo,
                string usuarioId)
        {
            return await _notificacaoRepository
                .ExisteNotificacaoPorChamadoTipoEUsuario(
                    chamadoId,
                    tipo,
                    usuarioId);
        }

        // MÉTODOS PRIVADOS

        // Centraliza a busca obrigatória usada pelas alterações de estado.
        private async Task<Notificacao>
            BuscarNotificacaoObrigatoria(int id)
        {
            var notificacao =
                await _notificacaoRepository.BuscarPorId(id);

            if (notificacao == null)
            {
                throw new DomainExceptionValidation(
                    "Notificação não encontrada.");
            }

            return notificacao;
        }

        // Reutiliza a busca obrigatória e valida o acesso ao destinatário.
        private async Task<Notificacao>
            BuscarNotificacaoEValidarAcesso(
                int id,
                string usuarioLogadoId,
                bool podeVisualizar)
        {
            var notificacao =
                await BuscarNotificacaoObrigatoria(id);

            var pertenceAoUsuario =
                notificacao.UsuarioId == usuarioLogadoId;

            if (!pertenceAoUsuario && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui acesso a esta notificação.");
            }

            return notificacao;
        }

        // Protege consultas que recebem diretamente o identificador do usuário.
        private static void ValidarAcessoAoUsuario(
            string usuarioId,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            if (string.IsNullOrWhiteSpace(usuarioId))
            {
                throw new DomainExceptionValidation(
                    "Usuário não encontrado.");
            }

            var proprioUsuario =
                usuarioId == usuarioLogadoId;

            if (!proprioUsuario && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui acesso às notificações deste usuário.");
            }
        }

        // Persiste o novo estado e atualiza a Topbar do destinatário.
        private async Task SalvarAtualizacao(
            Notificacao notificacao, bool agendarAtualizacaoTempoReal = true)
        {
            await _transacaoService.ExecutarAsync(async () =>
            {
                await _notificacaoRepository.Atualizar(notificacao);

                if (agendarAtualizacaoTempoReal)
                {
                    await _atualizacaoTempoRealService
                        .NotificarNotificacaoAtualizadaAsync(notificacao.UsuarioId);
                }
            });   
        }

        // Mantém a montagem da resposta paginada em um único ponto.
        private PaginacaoResponseDto<NotificacaoResponseDto>
            CriarRespostaPaginada(
                PaginacaoRequestDto request,
                (
                    IEnumerable<Notificacao> Notificacao,
                    int totalRegistro
                ) resultado)
        {
            return new PaginacaoResponseDto<NotificacaoResponseDto>
            {
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.totalRegistro,
                TotalPaginas = (int)Math.Ceiling(
                    resultado.totalRegistro /
                    (double)request.TamanhoPagina),
                Dados = _mapper.Map<IEnumerable<NotificacaoResponseDto>>(
                    resultado.Notificacao)
            };
        }
    }
}
