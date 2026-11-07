using Application.Dto.Chamado;
using Application.Dto.NotificacaoDto;
using Application.Dto.RegraDistribuicaoDto;
using Application.DTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena cadastro, critérios, validações e execução das regras de
    // distribuição automática ou aplicada dos chamados.
    public class RegraDistribuicaoService : IRegraDistribuicaoService
    {
        private readonly IMapper _mapper;
        private readonly IRegraDistribuicaoRepository _regraRepository;
        private readonly ICriterioDistribuicaoRepository _criterioRepository;
        private readonly IChamadoRepository _chamadoRepository;
        private readonly INotificacaoService _notificacaoService;
        private readonly IUsuarioPerfilService _usuarioPerfilService;
        private readonly IPerfilSlaService _perfilSlaService;
        private readonly ITransacaoService _transacaoService;
        private readonly IAtualizacaoTempoRealService _atualizacaoTempoReal;
        private readonly ICentralAtendimentoTempoRealService _centralAtendimentoTempoRealService;


        public RegraDistribuicaoService(
            IMapper mapper,
            IRegraDistribuicaoRepository regraRepository,
            ICriterioDistribuicaoRepository criterioRepository,
            IChamadoRepository chamadoRepository,
            INotificacaoService notificacaoService,
            IUsuarioPerfilService usuarioPerfilService,
            IPerfilSlaService perfilSlaService,
            ITransacaoService transacaoService,
            IAtualizacaoTempoRealService atualizacaoTempoReal,
            ICentralAtendimentoTempoRealService centralAtendimentoTempoRealService)
        {
            _mapper = mapper;
            _regraRepository = regraRepository;
            _criterioRepository = criterioRepository;
            _chamadoRepository = chamadoRepository;
            _notificacaoService = notificacaoService;
            _usuarioPerfilService = usuarioPerfilService;
            _perfilSlaService = perfilSlaService;
            _transacaoService = transacaoService;
            _atualizacaoTempoReal = atualizacaoTempoReal;
            _centralAtendimentoTempoRealService = centralAtendimentoTempoRealService;
        }

        // CADASTRO E ALTERAÇÃO

        // Valida se o destino possui período de SLA antes de persistir a regra.
        public async Task<RegraDistribuicaoResponseDto> Adicionar(
            RegraDistribuicaoRequestDto regraDto)
        {
            var regra = _mapper.Map<RegraDistribuicao>(regraDto);

            await ValidarPeriodoDestino(
                regraDto.TipoDestino,
                regraDto.UsuarioDestinoId,
                regraDto.PerfilDestinoId);

            var criar = await _regraRepository.Adicionar(regra);

            return _mapper.Map<RegraDistribuicaoResponseDto>(criar);
        }

        // Ao transformar a regra em automática e indefinida, também valida se
        // suas categorias já pertencem a outra regra principal.
        public async Task<RegraDistribuicaoResponseDto> Atualizar(
            int id,
            RegraDistribuicaoRequestDto regraDto)
        {
            var regra = await _regraRepository.BuscarPorId(id);

            if (regra == null)
                throw new DomainExceptionValidation("regra nao encontrada");

            await ValidarPeriodoDestino(
                regraDto.TipoDestino,
                regraDto.UsuarioDestinoId,
                regraDto.PerfilDestinoId);

            var seraRegraPrincipal =
                regraDto.Ativo &&
                regraDto.TipoRegra == TipoRegraEnum.Automatico &&
                regraDto.PeriodoIndefinido;

            if (seraRegraPrincipal)
            {
                await ValidarCriteriosComoRegraPrincipal(id);
            }

            regra.Atualizar(
                regraDto.Nome,
                regraDto.TipoRegra,
                regraDto.TipoDestino,
                regraDto.Ativo,
                regraDto.PeriodoIndefinido,
                regraDto.DataInicial,
                regraDto.DataFinal,
                regraDto.UsuarioDestinoId,
                regraDto.PerfilDestinoId);

            await _regraRepository.Atualizar(regra);

            return _mapper.Map<RegraDistribuicaoResponseDto>(regra);
        }

        // CONSULTAS

        public async Task<RegraDistribuicaoResponseDto> BuscarPorId(int id)
        {
            var regra = await _regraRepository.BuscarPorId(id);

            return _mapper.Map<RegraDistribuicaoResponseDto>(regra);
        }

        public async Task<IEnumerable<RegraDistribuicaoResponseDto>> BuscarTodos()
        {
            var regra = await _regraRepository.BuscarTodos();

            return _mapper.Map<IEnumerable<RegraDistribuicaoResponseDto>>(regra);
        }

        // ESTADO E REMOÇÃO

        public async Task Ativar(int id)
        {
            var regra = await _regraRepository.BuscarPorId(id);

            if (regra == null)
                throw new DomainExceptionValidation("Regra não encontrada!");

            await ValidarPeriodoDestino(
                regra.TipoDestino,
                regra.UsuarioDestinoId,
                regra.PerfilDestinoId);

            var seraRegraPrincipal =
                regra.TipoRegra == TipoRegraEnum.Automatico &&
                regra.PeriodoIndefinido;

            if (seraRegraPrincipal)
            {
                await ValidarCriteriosComoRegraPrincipal(
                    regra.Id_Regras);
            }

            regra.Ativar();

            await _regraRepository.Atualizar(regra);
        }

        public async Task Desativar(int id)
        {
            var regra = await _regraRepository.BuscarPorId(id);

            if (regra == null)
                throw new DomainExceptionValidation("Regra não encontrada");

            regra.Desativar();

            await _regraRepository.Atualizar(regra);
        }

        public async Task Remover(int id)
        {
            await _regraRepository.Remover(id);
        }

        // CRITÉRIOS

        public async Task RemoverCriterio(int criterioId)
        {
            var criterio = await _criterioRepository.BuscarPorId(criterioId);

            if (criterio == null)
                throw new DomainExceptionValidation("Criterio nao encontrado");

            await _criterioRepository.Remover(criterio.Id_Criterio);
        }

        public async Task<RegraDistribuicaoResponseDto> AdicionarCriterio(
            int id,
            CriterioDistribuicaoRequestDto criterio)
        {
            var regra = await _regraRepository.BuscarPorId(id);

            if (regra == null)
                throw new DomainExceptionValidation("Regra não encontrada.");

            if (criterio == null)
                throw new DomainExceptionValidation("Critério não encontrado.");

            if (regra.Ativo &&
                regra.TipoRegra == TipoRegraEnum.Automatico &&
                regra.PeriodoIndefinido)
            {
                await ValidarCategoriaPrincipalUnica(
                    regra.Id_Regras,
                    criterio.CategoriaAtendimentoId);
            }

            var novoCriterio = new CriterioDistribuicao(
                regra.Id_Regras,
                criterio.CategoriaAtendimentoId);

            regra.AdicionarCriterio(novoCriterio);

            await _criterioRepository.Adicionar(novoCriterio);

            return _mapper.Map<RegraDistribuicaoResponseDto>(regra);
        }

        // DISTRIBUIÇÃO AUTOMÁTICA

        // Localiza a regra automática da categoria e distribui o chamado
        // quando houver uma regra vigente.
        public async Task ProcessarDistribuicaoAutomatica(Chamado chamado)
        {
            var regra = await BuscarRegraAutomaticaPorCategoria(
                chamado.CategoriaAtendimentoId);

            if (regra == null)
                return;

            await DistribuirChamadoPelaRegra(chamado, regra);
        }

        private async Task<RegraDistribuicao?> BuscarRegraAutomaticaPorCategoria(
            int categoriaAtendimentoId)
        {
            var regras = await _regraRepository.BuscarTodos();

            return regras
                .Where(r =>
                    r.EhAutomatica() &&
                    r.Criterios.Any(c =>
                        c.CategoriaAtendimentoId == categoriaAtendimentoId))
                .OrderByDescending(r => r.PeriodoIndefinido)
                .FirstOrDefault();
        }

        // DISTRIBUIÇÃO APLICADA

        // Retorna somente chamados abertos e ainda sem técnico das categorias
        // configuradas na regra aplicada.
        public async Task<IEnumerable<ChamadoResponseDto>>
            BuscarChamadosDisponiveis(int regraId)
        {
            var regra = await _regraRepository.BuscarPorId(regraId);

            if (regra == null)
                throw new DomainExceptionValidation("Regra não encontrada.");

            if (!regra.Ativo)
                throw new DomainExceptionValidation("A regra está inativa.");

            if (regra.TipoRegra != TipoRegraEnum.Aplicada)
                throw new DomainExceptionValidation(
                    "Somente regras do tipo aplicada podem ser utilizadas.");

            var criterios = await _criterioRepository
                .BuscarCriterioPorRegra(regraId);

            var categoriasIds = criterios
                .Select(c => c.CategoriaAtendimentoId)
                .ToList();

            var chamados = await _chamadoRepository
                .BuscarChamadosPorCategorias(categoriasIds);

            chamados = chamados
                .Where(c =>
                    c.Status == StatusChamadoEnum.Aberto &&
                    string.IsNullOrWhiteSpace(c.TecnicoResponsavelId));

            return _mapper.Map<
                IEnumerable<ChamadoResponseDto>>(chamados);
        }

        // VALIDAÇÕES INTERNAS

        // Cada categoria pode possuir apenas uma regra automática indefinida.
        private async Task ValidarCategoriaPrincipalUnica(
            int regraId,
            int categoriaAtendimentoId)
        {
            var regras = await _regraRepository.BuscarTodos();

            var jaExisteRegraPrincipal = regras.Any(r =>
                r.Id_Regras != regraId &&
                r.Ativo &&
                r.TipoRegra == TipoRegraEnum.Automatico &&
                r.PeriodoIndefinido &&
                r.Criterios.Any(c =>
                    c.CategoriaAtendimentoId == categoriaAtendimentoId));

            if (jaExisteRegraPrincipal)
            {
                throw new DomainExceptionValidation(
                    "Já existe uma regra automática fixa para esta categoria.");
            }
        }

        private async Task ValidarCriteriosComoRegraPrincipal(int regraId)
        {
            var criterios = await _criterioRepository
                .BuscarCriterioPorRegra(regraId);

            foreach (var criterio in criterios)
            {
                await ValidarCategoriaPrincipalUnica(
                    regraId,
                    criterio.CategoriaAtendimentoId);
            }
        }

        private async Task ValidarPeriodoDestino(
            TipoDestinoEnum tipoDestino,
            string? usuarioDestinoId,
            int? perfilDestinoId)
        {
            if (tipoDestino == TipoDestinoEnum.Usuario)
            {
                if (string.IsNullOrWhiteSpace(usuarioDestinoId))
                    throw new DomainExceptionValidation(
                        "Usuário destino é obrigatório.");

                await _perfilSlaService.ObterPeriodoVigentePorUsuario(
                    usuarioDestinoId!);
                return;
            }

            if (tipoDestino == TipoDestinoEnum.Perfil)
            {
                if (!perfilDestinoId.HasValue)
                    throw new DomainExceptionValidation(
                        "Perfil destino é obrigatório.");

                await _perfilSlaService.ObterPeriodoVigentePorPerfil(
                    perfilDestinoId.Value);
                return;
            }

            throw new DomainExceptionValidation(
                "O tipo de destino da regra é inválido.");
        }

        // Valida todo o lote antes de distribuir qualquer chamado.
        public async Task AplicarRegraChamados(
             AplicarRegraChamadosRequestDto request)
        {
            if (request == null)
            {
                throw new DomainExceptionValidation(
                    "Dados para aplicação da regra são obrigatórios.");
            }

            if (request.RegraId <= 0)
                throw new DomainExceptionValidation("Regra inválida.");

            if (request.ChamadosId == null || !request.ChamadosId.Any())
            {
                throw new DomainExceptionValidation(
                    "Selecione ao menos um chamado.");
            }

            if (request.ChamadosId.Count > 10)
            {
                throw new DomainExceptionValidation(
                    "É permitido selecionar no máximo 10 chamados.");
            }

            if (request.ChamadosId.Distinct().Count() !=
                request.ChamadosId.Count)
            {
                throw new DomainExceptionValidation(
                    "A lista de chamados contém itens duplicados.");
            }

            var regra = await _regraRepository.BuscarPorId(
                request.RegraId);

            if (regra == null)
                throw new DomainExceptionValidation("Regra não encontrada.");

            if (!regra.Ativo)
                throw new DomainExceptionValidation("A regra está inativa.");

            if (regra.TipoRegra != TipoRegraEnum.Aplicada)
            {
                throw new DomainExceptionValidation(
                    "Somente regras do tipo aplicada podem ser utilizadas.");
            }

            if (!regra.EstaVigente())
            {
                throw new DomainExceptionValidation(
                    "A regra está fora do período de vigência.");
            }

            var chamados = new List<Chamado>();

            foreach (var chamadoId in request.ChamadosId)
            {
                var chamado = await _chamadoRepository
                    .BuscarDisponivelParaDistribuicao(chamadoId);

                if (chamado == null)
                {
                    throw new DomainExceptionValidation(
                        $"O chamado {chamadoId} não está mais disponível para distribuição.");
                }

                chamados.Add(chamado);
            }

            IReadOnlyCollection<string> destinatariosNotificacao;

            if (regra.TipoDestino == TipoDestinoEnum.Usuario)
            {
                if (string.IsNullOrWhiteSpace(regra.UsuarioDestinoId))
                {
                    throw new DomainExceptionValidation(
                        "A regra não possui um usuário destino.");
                }

                destinatariosNotificacao = new[]
                {
                    regra.UsuarioDestinoId
                };
            }
            else if (regra.TipoDestino == TipoDestinoEnum.Perfil)
            {
                if (!regra.PerfilDestinoId.HasValue)
                {
                    throw new DomainExceptionValidation(
                        "A regra não possui um perfil destino.");
                }

                destinatariosNotificacao = (
                    await _usuarioPerfilService.BuscarUsuariosPerfil(
                        regra.PerfilDestinoId.Value)
                )
                .Select(usuario => usuario.UsuarioId)
                .Where(usuarioId => !string.IsNullOrWhiteSpace(usuarioId))
                .Distinct()
                .ToArray();

                if (destinatariosNotificacao.Count == 0)
                {
                    throw new DomainExceptionValidation(
                        "O perfil destino não possui usuários para receber os chamados.");
                }
            }
            else
            {
                throw new DomainExceptionValidation(
                    "O tipo de destino da regra é inválido.");
            }

            var usuariosDashboard = chamados
                .Select(chamado => chamado.SolicitanteId)
                .Concat(destinatariosNotificacao)
                .Where(usuarioId => !string.IsNullOrWhiteSpace(usuarioId))
                .Select(usuarioId => usuarioId!)
                .Distinct()
                .ToArray();

            await _transacaoService.ExecutarAsync(async () =>
            {
                foreach (var chamado in chamados)
                {
                    await DistribuirChamadoPelaRegra(
                        chamado,
                        regra,
                        agendarAtualizacaoTempoReal: false);

                    var usuariosAfetados = new[]
                    {
                chamado.SolicitanteId ?? string.Empty
            }
                    .Concat(destinatariosNotificacao)
                    .Where(usuarioId => !string.IsNullOrWhiteSpace(usuarioId))
                    .Distinct()
                    .ToArray();

                    await _atualizacaoTempoReal
                        .NotificarFilaAtualizadaAsync(
                            chamado.Id_Chamado);

                    await _atualizacaoTempoReal
                        .NotificarChamadoAtualizadoAsync(
                            usuariosAfetados,
                            chamado.Id_Chamado);
                }

                await _atualizacaoTempoReal
                    .NotificarNotificacoesAtualizadasAsync(
                        destinatariosNotificacao);

                await _atualizacaoTempoReal
                    .NotificarDashboardAtualizadoAsync(
                        usuariosDashboard);

                await _atualizacaoTempoReal
                    .NotificarDashboardGlobalAtualizadoAsync();
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();
        }

        // EXECUÇÃO COMUM

        // Executa a distribuição comum para regras automáticas e aplicadas,
        // registrando período de SLA, auditoria e notificações.
        private async Task DistribuirChamadoPelaRegra(
            Chamado chamado,
            RegraDistribuicao regra,
             bool agendarAtualizacaoTempoReal = true)
        {
            if (regra.TipoDestino == TipoDestinoEnum.Usuario)
            {
                if (string.IsNullOrWhiteSpace(regra.UsuarioDestinoId))
                {
                    throw new DomainExceptionValidation(
                        "A regra não possui um usuário destino.");
                }

                var periodo = await _perfilSlaService
                    .ObterPeriodoVigentePorUsuario(
                        regra.UsuarioDestinoId);

                var statusAnterior = chamado.Status;

                chamado.DefinirPeriodoSla(periodo);
                chamado.DistribuirParaUsuario(regra.UsuarioDestinoId);

                var descricaoAuditoria = regra.TipoRegra ==
                    TipoRegraEnum.Automatico
                        ? "Chamado distribuído automaticamente para um usuário."
                        : "Chamado distribuído por uma regra aplicada para um usuário.";

                chamado.RegistrarAuditoria(
                    EventoAuditoriaChamadoEnum.Distribuicao,
                    null,
                    "SISTEMA",
                    null,
                    statusAnterior,
                    chamado.Status,
                    descricaoAuditoria,
                    null,
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        UsuarioDestinoId = regra.UsuarioDestinoId,
                        PeriodoTrabalhoPerfilId =
                            chamado.PeriodoTrabalhoPerfilId,
                        PerfilPeriodoSlaId = periodo.PerfilId
                    }));

                await _chamadoRepository.Atualizar(chamado);

                await _notificacaoService.Adicionar(
                    new NotificacaoRequestDto
                    {
                        usuarioId = regra.UsuarioDestinoId,
                        usuarioOrigemId = chamado.SolicitanteId!,
                        chamadoId = chamado.Id_Chamado,
                        tipo = TipoNotificacaoEnum.ChamadoDistribuidoUsuario,
                        titulo = "Novo chamado distribuído",
                        mensagem =
                            $"O chamado {chamado.Numero} foi distribuído para você.",
                        status = StatusNotificacaoEnum.Pendente,
                      
                    }, agendarAtualizacaoTempoReal);

                return;
            }

            if (regra.TipoDestino == TipoDestinoEnum.Perfil)
            {
                if (!regra.PerfilDestinoId.HasValue)
                {
                    throw new DomainExceptionValidation(
                        "A regra não possui um perfil destino.");
                }

                var usuariosPerfil = (
                    await _usuarioPerfilService.BuscarUsuariosPerfil(
                        regra.PerfilDestinoId.Value)
                ).ToList();

                if (!usuariosPerfil.Any())
                {
                    if (regra.TipoRegra == TipoRegraEnum.Automatico)
                        return;

                    throw new DomainExceptionValidation(
                        "O perfil destino não possui usuários para receber os chamados.");
                }

                var periodo = await _perfilSlaService
                    .ObterPeriodoVigentePorPerfil(
                        regra.PerfilDestinoId.Value);


                var statusAnterior = chamado.Status;

                chamado.DefinirPeriodoSla(periodo);
                chamado.DistribuirParaPerfil();

                var descricaoAuditoria = regra.TipoRegra ==
                    TipoRegraEnum.Automatico
                        ? "Chamado distribuído automaticamente para um perfil."
                        : "Chamado distribuído por uma regra aplicada para um perfil.";

                chamado.RegistrarAuditoria(
                    EventoAuditoriaChamadoEnum.Distribuicao,
                    null,
                    "SISTEMA",
                    null,
                    statusAnterior,
                    chamado.Status,
                    descricaoAuditoria,
                    null,
                    System.Text.Json.JsonSerializer.Serialize(new
                    {
                        PerfilDestinoId = regra.PerfilDestinoId,
                        PeriodoTrabalhoPerfilId =
                            chamado.PeriodoTrabalhoPerfilId,
                        PerfilPeriodoSlaId = periodo.PerfilId
                    }));

                await _chamadoRepository.Atualizar(chamado);

                foreach (var usuario in usuariosPerfil)
                {
                    await _notificacaoService.Adicionar(
                        new NotificacaoRequestDto
                        {
                            usuarioId = usuario.UsuarioId,
                            usuarioOrigemId = chamado.SolicitanteId!,
                            chamadoId = chamado.Id_Chamado,
                            tipo = TipoNotificacaoEnum.ChamadoDistribuidoPerfil,
                            titulo = "Novo chamado disponível",
                            mensagem =
                                $"O chamado {chamado.Numero} foi direcionado para seu perfil.",
                            status = StatusNotificacaoEnum.Pendente
                        },agendarAtualizacaoTempoReal);
                }

                return;
            }

            throw new DomainExceptionValidation(
                "O tipo de destino da regra é inválido.");
        }
    }
}

