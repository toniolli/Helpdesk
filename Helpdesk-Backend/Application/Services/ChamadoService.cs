using Application.Dto.Chamado;
using Application.Dto.EmailNotification;
using Application.Dto.NotificacaoDto;
using Application.Dto.Paginacao;
using Application.Interfaces;
using Application.ViewModel;
using AutoMapper;
using Domain.Entities;
using Domain.Enum;
using Domain.Interfaces;
using Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    // Orquestra os casos de uso de chamados. As regras de status, pausas e SLA permanecem
    // na entidade Chamado; este serviço coordena persistência, auditoria, notificações e e-mails.
    // Ao realizar manutenção, preserve a ordem atual entre salvar dados e emitir efeitos externos.
    public class ChamadoService : IChamadoService
    {
        private readonly IMapper _mapper;
        private readonly IChamadoRepository _chamadoRepository;
        private readonly ICategoriaAtendimentoRepository _categoriaRepository;
        private readonly IusuarioService _usuarioService;
        private readonly IEmailNotificationService _emailNotificationService;
        private readonly IEnviarEmail _enviarEmail;
        private readonly INotificacaoService _notificacaoService;
        private readonly IRegraDistribuicaoService _regraDistribuicaoService;
        private readonly IUsuarioPerfilService _usuarioPerfilService;
        private readonly IAtualizacaoTempoRealService _atualizacaoTempoReal;
        private readonly IPerfilService _perfilService;
        private readonly IPerfilSlaService _perfilSlaService;
        private readonly ITransacaoService _transacaoService;
        private readonly ICentralAtendimentoTempoRealService _centralAtendimentoTempoRealService;

        public ChamadoService(
            IMapper mapper,
            IChamadoRepository chamadoRepository,
            ICategoriaAtendimentoRepository categoriaRepository,
            IusuarioService usuarioService,
            IEmailNotificationService emailNotificationService,
            IEnviarEmail enviarEmail,
            INotificacaoService notificacaoService,
            IRegraDistribuicaoService regraDistribuicaoService,
            IUsuarioPerfilService usuarioPerfilService,
            IAtualizacaoTempoRealService atualizacaoTempoRealService,
            IPerfilService perfilService,
            IPerfilSlaService perfilSlaService,
            ITransacaoService transacaoService,
            ICentralAtendimentoTempoRealService centralAtendimentoTempoRealService)
        {
            _mapper = mapper;
            _chamadoRepository = chamadoRepository;
            _categoriaRepository = categoriaRepository;
            _usuarioService = usuarioService;
            _emailNotificationService = emailNotificationService;
            _enviarEmail = enviarEmail;
            _notificacaoService = notificacaoService;
            _regraDistribuicaoService = regraDistribuicaoService;
            _usuarioPerfilService = usuarioPerfilService;
            _atualizacaoTempoReal = atualizacaoTempoRealService;
            _perfilService = perfilService;
            _perfilSlaService = perfilSlaService;
            _transacaoService = transacaoService;
            _centralAtendimentoTempoRealService = centralAtendimentoTempoRealService;
        }

        // ==========================
        // CRIAÇÃO E MANUTENÇÃO
        // ==========================

        public async Task<ChamadoResponseDto> Criar(
             string solicitanteId,
             ChamadoRequestDto dto)
        {
            var categoria = await _categoriaRepository.BuscarPorId(
                dto.CategoriaAtendimentoId);

            var solicitante = await _usuarioService.BuscarPorIdInterno(
                solicitanteId);

            if (categoria == null)
            {
                throw new DomainExceptionValidation(
                    "Categoria de atendimento não encontrada.");
            }

            var ano = DateTime.Now.Year;

            var ultimoSequencial =
                await _chamadoRepository.BuscarUltimoSequencialPorAno(
                    ano);

            var numero =
                $"CH-{ano}-{(ultimoSequencial + 1):D6}";

            var chamado = new Chamado(
                solicitanteId,
                dto.CategoriaAtendimentoId,
                dto.EquipamentoAfetado,
                dto.Descricao,
                categoria.Prioridade);

            chamado.DefinirNumero(numero);

            var perfilSolicitante = await ObterNomePerfil(
                solicitante.Id_Perfil);

            Chamado criar = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                chamado.RegistrarAuditoria(
                    EventoAuditoriaChamadoEnum.Criacao,
                    solicitante.Id,
                    solicitante.Username,
                    perfilSolicitante,
                    null,
                    StatusChamadoEnum.Aberto,
                    "Chamado criado.",
                    null,
                    JsonSerializer.Serialize(new
                    {
                        chamado.Numero,
                        chamado.CategoriaAtendimentoId,
                        chamado.EquipamentoAfetado,
                        chamado.Descricao,
                        chamado.Prioridade
                    }));

                criar = await _chamadoRepository.Adicionar(
                    chamado);

                await _regraDistribuicaoService
                    .ProcessarDistribuicaoAutomatica(
                        criar);

                await _notificacaoService.Adicionar(
                    new NotificacaoRequestDto
                    {
                        usuarioId = criar.SolicitanteId,
                        usuarioOrigemId = criar.SolicitanteId,
                        chamadoId = criar.Id_Chamado,
                        tipo = TipoNotificacaoEnum.ChamadoCriado,
                        titulo = "Chamado registrado!",
                        mensagem =
                            $"Seu chamado {criar.Numero} foi registrado com sucesso.",
                        status = StatusNotificacaoEnum.Pendente
                    });

                if (criar.Status == StatusChamadoEnum.Aberto)
                {
                    await _atualizacaoTempoReal
                        .NotificarFilaAtualizadaAsync(
                            criar.Id_Chamado);
                }

                var usuariosAfetados = new[]
                {
                    criar.SolicitanteId ?? string.Empty,
                    criar.TecnicoResponsavelId ?? string.Empty
                };
                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    criar.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();


            // O e-mail fica fora da transação principal.
            var emailNotificacao =
                await _emailNotificationService.Adicionar(
                    new EmailNotificationRequestDto
                    {
                        ChamadoId = criar.Id_Chamado,
                        Tipo = TipoNotificacaoEmailEnum
                            .AberturaChamadoSolicitante,
                        Destinatario = solicitante.Email,
                        Assunto =
                            $"Chamado {numero} registrado com sucesso"
                    });

            try
            {
                var email = _enviarEmail.CriarEmailAberturaChamado(
                    solicitante.Email,
                    solicitante.Username,
                    criar.Numero);

                await _enviarEmail.EnviarEmail(email);

                await _emailNotificationService
                    .AtualizarStatusEnviado(
                        emailNotificacao.Id_EmailNotfication);
            }
            catch (Exception ex)
            {
                await _emailNotificationService
                    .AtualizarStatusFalhou(
                        emailNotificacao.Id_EmailNotfication,
                        ex.Message);
            }

            return _mapper.Map<ChamadoResponseDto>(
                criar);
        }

        public async Task<ChamadoResponseDto> Atualizar(
              int id,
              ChamadoRequestDto dto,
              string usuarioLogadoId,
              bool podevisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(id);

            if (chamado == null)
            {
                throw new DomainExceptionValidation(
                    "Chamado não encontrado.");
            }

            var participaDoChamado =
                chamado.SolicitanteId == usuarioLogadoId ||
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!participaDoChamado && !podevisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui essa permissão");
            }

            var categoria = await _categoriaRepository.BuscarPorId(
                dto.CategoriaAtendimentoId);

            if (categoria == null)
            {
                throw new DomainExceptionValidation(
                    "Categoria de atendimento não encontrada.");
            }

            var detalhesAnteriores = JsonSerializer.Serialize(new
            {
                chamado.Descricao,
                chamado.EquipamentoAfetado,
                chamado.CategoriaAtendimentoId,
                chamado.Prioridade
            });

            var houveAlteracao =
                chamado.Descricao != dto.Descricao ||
                chamado.EquipamentoAfetado != dto.EquipamentoAfetado ||
                chamado.CategoriaAtendimentoId !=
                    dto.CategoriaAtendimentoId ||
                chamado.Prioridade != categoria.Prioridade;

            Chamado atualizar = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                chamado.AlterarDescricao(dto.Descricao);

                chamado.AlterarEquipamentoAfetado(
                    dto.EquipamentoAfetado);

                chamado.AlterarCategoria(
                    dto.CategoriaAtendimentoId,
                    categoria.Prioridade);

                if (houveAlteracao)
                {
                    var usuario = await _usuarioService
                        .BuscarPorIdInterno(usuarioLogadoId);

                    var perfil = await ObterNomePerfil(
                        usuario.Id_Perfil);

                    chamado.RegistrarAuditoria(
                        EventoAuditoriaChamadoEnum.Alteracao,
                        usuario.Id,
                        usuario.Username,
                        perfil,
                        chamado.Status,
                        chamado.Status,
                        "Dados do chamado alterados.",
                        detalhesAnteriores,
                        JsonSerializer.Serialize(new
                        {
                            chamado.Descricao,
                            chamado.EquipamentoAfetado,
                            chamado.CategoriaAtendimentoId,
                            chamado.Prioridade
                        }));
                }

                atualizar = await _chamadoRepository.Atualizar(
                    chamado);

                if (chamado.Status == StatusChamadoEnum.Aberto)
                {
                    await _atualizacaoTempoReal
                        .NotificarFilaAtualizadaAsync(
                            chamado.Id_Chamado);
                }

                var usuariosAfetados = new[]
                {
            chamado.SolicitanteId ?? string.Empty,
            chamado.TecnicoResponsavelId ?? string.Empty
        };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            return _mapper.Map<ChamadoResponseDto>(
                atualizar);
        }


        // ==========================
        // CONSULTAS GERAIS
        // ==========================

        public async Task<ChamadoResponseDto?> BuscarPorId(
            int id,
            string usuarioId,
            bool podeVisualizarTodos)
        {
            var chamado = await _chamadoRepository.BuscarPorId(id);

            if (chamado == null)
                return null;

            var participaDoChamado =
                chamado.SolicitanteId == usuarioId ||
                chamado.TecnicoResponsavelId == usuarioId;

            if (!podeVisualizarTodos && !participaDoChamado)
            {
                throw new AcessoNegadoException("Você não possui acesso a este chamado!");
            }

            return _mapper.Map<ChamadoResponseDto>(chamado);
        }


        public async Task<IEnumerable<ChamadoResponseDto>> BuscarTodos()
        {
            var chamado = await _chamadoRepository.BuscarTodos();
            var response = _mapper.Map<IEnumerable<ChamadoResponseDto>>(chamado);
            foreach (var chamados in response)
            {
                if (!string.IsNullOrEmpty(chamados.SolicitanteId))
                {
                    var solicitante = await _usuarioService.BuscarPorIdInterno(
                        chamados.SolicitanteId

                    );
                    chamados.SolicitanteNome = solicitante.Username;
                }
                if (!string.IsNullOrEmpty(chamados.TecnicoResponsavelId))
                {
                    var tecnico = await _usuarioService.BuscarPorIdInterno(
                        chamados.TecnicoResponsavelId

                    );
                    chamados.TecnicoNome = tecnico.Username;
                }

                chamados.PrioridadeNome = chamados.Prioridade.ToString();
            }
            return response;
        }
        // ==========================
        // ATRIBUIÇÃO E TRANSFERÊNCIA
        // ==========================

        public async Task<ChamadoResponseDto> TransferirChamado(
            int chamadoId,
            string destinoId,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(
                chamadoId);

            if (chamado == null)
            {
                throw new DomainExceptionValidation(
                    "Chamado não encontrado.");
            }

            var ehTecnicoResponsavel =
                chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!ehTecnicoResponsavel && !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Somente o técnico responsável pode transferir o chamado.");
            }

            if (chamado.Status != StatusChamadoEnum.EmAtendimento)
            {
                throw new DomainExceptionValidation(
                    "Somente chamados em atendimento podem ser transferidos.");
            }

            if (string.IsNullOrWhiteSpace(
                    chamado.TecnicoResponsavelId))
            {
                throw new DomainExceptionValidation(
                    "O chamado não possui técnico responsável.");
            }

            if (chamado.TecnicoResponsavelId == destinoId)
            {
                throw new DomainExceptionValidation(
                    "Não é possível transferir o chamado para o mesmo técnico.");
            }

            var tecnicoDestino =
                await _usuarioService.BuscarPorIdInterno(destinoId);

            if (tecnicoDestino == null)
            {
                throw new DomainExceptionValidation(
                    "Técnico destino não encontrado.");
            }

            var tecnicoOrigemId =
                chamado.TecnicoResponsavelId;

            await _transacaoService.ExecutarAsync(async () =>
            {
                await _notificacaoService.Adicionar(
                    new NotificacaoRequestDto
                    {
                        usuarioId = destinoId,
                        usuarioOrigemId = tecnicoOrigemId,
                        chamadoId = chamado.Id_Chamado,
                        tipo =
                            TipoNotificacaoEnum.ChamadoTransferidoUsuario,
                        titulo = "Solicitação de transferência",
                        mensagem =
                            $"O chamado {chamado.Numero} foi transferido para sua análise. Deseja assumir este chamado?",
                        status = StatusNotificacaoEnum.Pendente
                    });

                await _atualizacaoTempoReal
                    .NotificarChamadoAtualizadoAsync(
                        new[]
                        {
                    chamado.SolicitanteId ?? string.Empty,
                    tecnicoOrigemId ?? string.Empty,
                    destinoId
                        },
                        chamado.Id_Chamado);
            });

            return _mapper.Map<ChamadoResponseDto>(
                chamado);
        }

        public async Task<ChamadoResponseDto> AceitarChamado(
            int chamadoId,
            int notificacaoId,
            string usuarioLogadoId,
            bool podeVisualizarTodos)
        {
            var notificacao = await _notificacaoService.BuscarPorIdInterno(notificacaoId);

            if (notificacao == null)
                throw new DomainExceptionValidation("Notificação não encontrada!");

            var pertenceAoUsuario = notificacao.UsuarioId == usuarioLogadoId;

            if (!pertenceAoUsuario && !podeVisualizarTodos)
            {
                throw new AcessoNegadoException("Você não possui acesso a esta notificação.");
            }

            if (notificacao.ChamadoId != chamadoId)
            {
                throw new DomainExceptionValidation("A notificação não pertence ao chamado informado.");
            }

            if (notificacao.Status != StatusNotificacaoEnum.Pendente)
            {
                throw new DomainExceptionValidation("A solicitação já foi processada.");
            }

            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation("Chamado não encontrado!");

            var usuario = await _usuarioService
                .BuscarPorIdInterno(usuarioLogadoId);
            var perfil = await ObterNomePerfil(usuario.Id_Perfil);

            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var tecnicoAnteriorId = chamado.TecnicoResponsavelId;
                var statusAnterior = chamado.Status;

                chamado.TransferirChamado(notificacao.UsuarioId);

                chamado.RegistrarAuditoria(
                    EventoAuditoriaChamadoEnum.Transferencia,
                    usuario.Id,
                    usuario.Username,
                    perfil,
                    statusAnterior,
                    chamado.Status,
                    "Transferência de chamado aceita.",
                    JsonSerializer.Serialize(new
                    {
                        TecnicoResponsavelId = tecnicoAnteriorId
                    }),
                    JsonSerializer.Serialize(new
                    {
                        chamado.TecnicoResponsavelId
                    }));

                chamadoAtualizado = await _chamadoRepository.Atualizar(chamado);

                await _notificacaoService.AceitarInterno(notificacaoId);
                await _notificacaoService.MarcarComoLidaInterno(notificacaoId);

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    tecnicoAnteriorId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });
            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            return _mapper.Map<ChamadoResponseDto>(chamadoAtualizado);
        }

        public async Task RecusarChamado(
            int notificacaoId,
            string usuarioLogadoId,
            bool podevisualizar)
        {
            var notificacao = await _notificacaoService.BuscarPorIdInterno(notificacaoId);

            if (notificacao == null)
                throw new DomainExceptionValidation("Notificação não encontrada!");

            var pertenceAoUsuario = notificacao.UsuarioId == usuarioLogadoId;

            if (!pertenceAoUsuario && !podevisualizar)
            {
                throw new AcessoNegadoException(
                    "Você não possui acesso a esta notificação.");
            }

            if (notificacao.Status != StatusNotificacaoEnum.Pendente)
                throw new DomainExceptionValidation("A solicitação já foi processada.");

            await _transacaoService.ExecutarAsync(async () =>
            {
                await _notificacaoService.RecusarInterno(notificacaoId);
                await _notificacaoService.MarcarComoLidaInterno(notificacaoId);
            });
        }


        public async Task<ChamadoResponseDto> AssumirAtendimento(
            int chamadoId,
            string tecnicoId)
        {
            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation("Chamado não encontrado.");

            var tecnico = await _usuarioService.BuscarPorIdInterno(tecnicoId);

            if (tecnico == null)
                throw new DomainExceptionValidation("Tecnico nao encontrado.");

            var estavaNaFila = chamado.Status == StatusChamadoEnum.Aberto;

            var distribuicaoParaPerfil = chamado.Status == StatusChamadoEnum.Distribuido &&
                string.IsNullOrWhiteSpace(chamado.TecnicoResponsavelId);

            IEnumerable<NotificacaoResponseDto> notificacaoPerfil = Enumerable.Empty<NotificacaoResponseDto>();

            if (distribuicaoParaPerfil)
            {
                notificacaoPerfil = await _notificacaoService.BuscarPendentesDistribuicaoPerfil(chamadoId);

                var tecnicoFoiNotificado = notificacaoPerfil.Any(n => n.UsuarioId == tecnicoId);

                if (!tecnicoFoiNotificado)
                    throw new DomainExceptionValidation("Este chamado foi distribuído para outro perfil.");
            }

            PeriodoTrabalhoPerfil? periodo = null;
            if (!chamado.PeriodoTrabalhoPerfilId.HasValue)
            {
                periodo = await _perfilSlaService
                    .ObterPeriodoVigentePorUsuario(tecnicoId);
            }

            var perfil = await ObterNomePerfil(tecnico.Id_Perfil);
            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var statusAnterior = chamado.Status;

                if (periodo != null)
                {
                    chamado.DefinirPeriodoSla(periodo);
                }

                chamado.AssumirAtendimento(tecnicoId);

                chamado.RegistrarAuditoria(
                    EventoAuditoriaChamadoEnum.InicioAtendimento,
                    tecnico.Id,
                    tecnico.Username,
                    perfil,
                    statusAnterior,
                    chamado.Status,
                    "Atendimento iniciado.",
                    null,
                    JsonSerializer.Serialize(new
                    {
                        chamado.TecnicoResponsavelId,
                        chamado.DataPrimeiroAtendimento,
                        chamado.PeriodoTrabalhoPerfilId,
                        PerfilPeriodoSlaId =
                            chamado.PeriodoTrabalhoPerfil?.PerfilId
                    }));

                chamadoAtualizado = await _chamadoRepository.Atualizar(chamado);

                if (distribuicaoParaPerfil)
                {
                    foreach (var notificacao in notificacaoPerfil)
                    {
                        if (notificacao.UsuarioId == tecnicoId)
                        {
                            await _notificacaoService.AceitarInterno(notificacao.Id_Notificacao);
                        }
                        else
                        {
                            await _notificacaoService.ExpirarInterno(notificacao.Id_Notificacao);
                        }

                        await _notificacaoService.MarcarComoLidaInterno(notificacao.Id_Notificacao);
                    }
                }

                // Notificação de confirmação para o técnico que assumiu o chamado.
                await _notificacaoService.Adicionar(new NotificacaoRequestDto
                {
                    usuarioId = chamado.TecnicoResponsavelId,
                    usuarioOrigemId = chamado.TecnicoResponsavelId,
                    chamadoId = chamado.Id_Chamado,
                    tipo = TipoNotificacaoEnum.ChamadoAssumido,
                    titulo = "Novo chamado atribuido!",
                    mensagem = $"Voce recebeu o chamado{chamado.Numero} para atendimento!",
                    status = StatusNotificacaoEnum.Aceita

                });

                if (estavaNaFila)
                {
                    await _atualizacaoTempoReal
                        .NotificarFilaAtualizadaAsync(chamado.Id_Chamado);
                }

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            // O registro e o envio de e-mail acontecem depois do commit do chamado e do Outbox.
            var emailNotificacao = await _emailNotificationService.Adicionar(new EmailNotificationRequestDto
            {
                ChamadoId = chamado.Id_Chamado,
                Tipo = TipoNotificacaoEmailEnum.AberturaChamadoTecnico,
                Destinatario = tecnico.Email,
                Assunto = $"Novo chamado atribuído - {chamado.Numero}"
            });

            try
            {
                var email = _enviarEmail.CriarEmailAtribuicaoChamado(
                    tecnico.Email,
                    tecnico.Username,
                    chamado.Numero);
                await _enviarEmail.EnviarEmail(email);

                await _emailNotificationService.AtualizarStatusEnviado(emailNotificacao.Id_EmailNotfication);

            }
            catch (Exception ex)
            {
                await _emailNotificationService.AtualizarStatusFalhou(
                    emailNotificacao.Id_EmailNotfication,
                    ex.Message);
            }

            return _mapper.Map<ChamadoResponseDto>(chamadoAtualizado);
        }

        // ==========================
        // TRANSIÇÕES DO ATENDIMENTO
        // ==========================

        public async Task<ChamadoResponseDto> AguardarRetorno(
            int chamadoId,
            string usuarioLogadoId,
            bool podevisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation("Chamado não encontrado.");

            var participaDoChamado =
              chamado.SolicitanteId == usuarioLogadoId ||
              chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!participaDoChamado && !podevisualizar)
            {
                throw new AcessoNegadoException("Você não possui essa permissão");
            }

            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var statusAnterior = chamado.Status;

                chamado.AguardarRetorno();

                await RegistrarAuditoriaUsuario(
                    chamado,
                    EventoAuditoriaChamadoEnum.Alteracao,
                    usuarioLogadoId,
                    statusAnterior,
                    chamado.Status,
                    "Chamado colocado em espera pelo retorno do solicitante.");

                await _notificacaoService.Adicionar(
                    new NotificacaoRequestDto
                    {
                        usuarioId = chamado.SolicitanteId,
                        usuarioOrigemId = chamado.TecnicoResponsavelId,
                        chamadoId = chamado.Id_Chamado,
                        tipo = TipoNotificacaoEnum.AguardandoRetorno,
                        titulo = "Chamado aguardando retorno",
                        mensagem =
                            $"O chamado {chamado.Numero} está aguardando sua resposta.",
                        status = StatusNotificacaoEnum.Pendente
                    });

                chamadoAtualizado =
                    await _chamadoRepository.Atualizar(chamado);

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            return _mapper.Map<ChamadoResponseDto>(chamadoAtualizado);
        }
        public async Task<ChamadoResponseDto> RetomarAtendimento(
            int chamadoId,
            string usuarioLogadoId,
            bool podevisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation("Chamado não encontrado.");

            var participaDoChamado =
              chamado.SolicitanteId == usuarioLogadoId ||
              chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!participaDoChamado && !podevisualizar)
            {
                throw new AcessoNegadoException("Você não possui essa permissão");
            }

            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var statusAnterior = chamado.Status;

                chamado.RetomarAtendimento();

                await RegistrarAuditoriaUsuario(
                    chamado,
                    EventoAuditoriaChamadoEnum.Alteracao,
                    usuarioLogadoId,
                    statusAnterior,
                    chamado.Status,
                    "Atendimento retomado após retorno do solicitante.");

                await _notificacaoService.Adicionar(
                    new NotificacaoRequestDto
                    {
                        usuarioId = chamado.TecnicoResponsavelId,
                        usuarioOrigemId = chamado.SolicitanteId,
                        chamadoId = chamado.Id_Chamado,
                        tipo = TipoNotificacaoEnum.RetornoRecebido,
                        titulo = "Retorno recebido",
                        mensagem =
                            $"O solicitante respondeu o chamado {chamado.Numero}.",
                        status = StatusNotificacaoEnum.Pendente
                    });

                chamadoAtualizado =
                    await _chamadoRepository.Atualizar(chamado);

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            return _mapper.Map<ChamadoResponseDto>(chamadoAtualizado);
        }


        public async Task<ChamadoResponseDto> EnviarEquipamento(
            int chamadoId,
            string usuarioLogadoId)
        {
            var chamado =
                await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
            {
                throw new DomainExceptionValidation(
                    "Chamado não encontrado.");
            }

            var ehSolicitante =
                chamado.SolicitanteId == usuarioLogadoId;

            if (!ehSolicitante)
            {
                throw new AcessoNegadoException(
                    "Somente o solicitante do chamado pode informar o envio do equipamento.");
            }

            if (string.IsNullOrWhiteSpace(
                chamado.TecnicoResponsavelId))
            {
                throw new DomainExceptionValidation(
                    "O chamado não possui técnico responsável.");
            }

            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var statusAnterior = chamado.Status;

                chamado.AguardarEquipamento();

                await RegistrarAuditoriaUsuario(
                    chamado,
                    EventoAuditoriaChamadoEnum.EquipamentoEnviado,
                    usuarioLogadoId,
                    statusAnterior,
                    chamado.Status,
                    "Solicitante informou o envio do equipamento para a matriz.");

                chamadoAtualizado =
                    await _chamadoRepository.Atualizar(chamado);

                await _notificacaoService.Adicionar(
                    new NotificacaoRequestDto
                    {
                        usuarioId =
                            chamado.TecnicoResponsavelId,

                        usuarioOrigemId =
                            chamado.SolicitanteId,

                        chamadoId =
                            chamado.Id_Chamado,

                        tipo =
                            TipoNotificacaoEnum.EquipamentoEnviado,

                        titulo =
                            "Equipamento enviado",

                        mensagem =
                            $"O solicitante informou o envio do equipamento do chamado {chamado.Numero}.",

                        status =
                            StatusNotificacaoEnum.Pendente
                    });

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            return _mapper.Map<ChamadoResponseDto>(
                chamadoAtualizado);
        }

        public async Task<ChamadoResponseDto> ReceberEquipamento(
            int chamadoId,
            string usuarioLogadoId,
            bool podeVisualizar)
        {
            var chamado =
                await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
            {
                throw new DomainExceptionValidation(
                    "Chamado não encontrado.");
            }

            var ehTecnicoResponsavel =
                chamado.TecnicoResponsavelId ==
                usuarioLogadoId;

            if (!ehTecnicoResponsavel &&
                !podeVisualizar)
            {
                throw new AcessoNegadoException(
                    "Somente o técnico responsável pode confirmar o recebimento do equipamento.");
            }

            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var statusAnterior = chamado.Status;

                chamado.ReceberEquipamento();

                await RegistrarAuditoriaUsuario(
                    chamado,
                    EventoAuditoriaChamadoEnum.EquipamentoRecebido,
                    usuarioLogadoId,
                    statusAnterior,
                    chamado.Status,
                    "Recebimento do equipamento confirmado.");

                chamadoAtualizado =
                    await _chamadoRepository.Atualizar(chamado);

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            return _mapper.Map<ChamadoResponseDto>(
                chamadoAtualizado);
        }
        // ==========================
        // ENCERRAMENTO E CANCELAMENTO
        // ==========================

        public async Task<ChamadoResponseDto> FecharChamado(
            int chamadoId,
            FecharChamadoDto dto,
            string usuarioLogadoId,
            bool podevisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation("Chamado não encontrado.");

            var participaDoChamado =
              chamado.SolicitanteId == usuarioLogadoId ||
              chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!participaDoChamado && !podevisualizar)
            {
                throw new AcessoNegadoException("Você não possui essa permissão");
            }

            var solicitante = await _usuarioService.BuscarPorIdInterno(chamado.SolicitanteId);
            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var statusAnterior = chamado.Status;

                chamado.Fechar(
                    dto.TipoAtendimento,
                    dto.TipoEncerramento,
                    dto.Solucao);

                await RegistrarAuditoriaUsuario(
                    chamado,
                    EventoAuditoriaChamadoEnum.Encerramento,
                    usuarioLogadoId,
                    statusAnterior,
                    chamado.Status,
                    "Chamado encerrado.",
                    null,
                    JsonSerializer.Serialize(new
                    {
                        dto.TipoAtendimento,
                        dto.TipoEncerramento,
                        dto.Solucao,
                        chamado.DataFechamento
                    }));

                chamadoAtualizado = await _chamadoRepository.Atualizar(chamado);

                // Notificação de encerramento para o solicitante.
                await _notificacaoService.Adicionar(new NotificacaoRequestDto
                {
                    usuarioId = chamado.SolicitanteId,
                    usuarioOrigemId = chamado.TecnicoResponsavelId,
                    chamadoId = chamado.Id_Chamado,
                    tipo = TipoNotificacaoEnum.ChamadoEncerrado,
                    titulo = "Chamado encerrado!",
                    mensagem = $"Seu chamado{chamado.Numero}foi encerrado!",
                    status = StatusNotificacaoEnum.Encerrado

                });

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            // O status do envio de e-mail é persistido independentemente do encerramento.
            var emailNotificacao = await _emailNotificationService.Adicionar(new EmailNotificationRequestDto
            {

                ChamadoId = chamado.Id_Chamado,
                Tipo = TipoNotificacaoEmailEnum.EncerramentoChamado,
                Destinatario = solicitante.Email,
                Assunto = $"Chamado {chamado.Numero} encerrado"
            });

            try
            {
                var email = _enviarEmail.CriarEmailEncerramentoChamado(
                    solicitante.Email,
                    solicitante.Username,
                    chamado.Numero);
                await _enviarEmail.EnviarEmail(email);

                await _emailNotificationService.AtualizarStatusEnviado(emailNotificacao.Id_EmailNotfication);

            }
            catch (Exception ex)
            {
                await _emailNotificationService.AtualizarStatusFalhou(
                    emailNotificacao.Id_EmailNotfication,
                    ex.Message);
            }

            return _mapper.Map<ChamadoResponseDto>(chamadoAtualizado);
        }



        public async Task<ChamadoResponseDto> CancelarChamado(
            int chamadoId,
            string usuarioLogadoId,
            bool podevisualizar)
        {
            var chamado = await _chamadoRepository.BuscarPorId(chamadoId);

            if (chamado == null)
                throw new DomainExceptionValidation("Chamado não encontrado.");

            var participaDoChamado =
               chamado.SolicitanteId == usuarioLogadoId ||
               chamado.TecnicoResponsavelId == usuarioLogadoId;

            if (!participaDoChamado && !podevisualizar)
            {
                throw new AcessoNegadoException("Você não possui essa permissão");
            }

            Chamado chamadoAtualizado = null!;

            await _transacaoService.ExecutarAsync(async () =>
            {
                var estavaNaFila = chamado.Status == StatusChamadoEnum.Aberto;
                var statusAnterior = chamado.Status;

                chamado.Cancelar();

                await RegistrarAuditoriaUsuario(
                    chamado,
                    EventoAuditoriaChamadoEnum.Cancelamento,
                    usuarioLogadoId,
                    statusAnterior,
                    chamado.Status,
                    "Chamado cancelado.");

                chamadoAtualizado = await _chamadoRepository.Atualizar(chamado);

                if (estavaNaFila)
                {
                    await _atualizacaoTempoReal
                        .NotificarFilaAtualizadaAsync(chamado.Id_Chamado);
                }

                var usuariosAfetados = new[]
                {
                    chamado.SolicitanteId ?? string.Empty,
                    chamado.TecnicoResponsavelId ?? string.Empty
                };

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();

            return _mapper.Map<ChamadoResponseDto>(chamadoAtualizado);
        }



        public async Task Remover(int id)
        {
            var chamado = await _chamadoRepository.BuscarPorId(id);

            if (chamado == null)
                throw new DomainExceptionValidation("Chamado não encontrado.");

            var estavaNaFila = chamado.Status == StatusChamadoEnum.Aberto;

            var usuariosAfetados = new[]
            {
                chamado.SolicitanteId ?? string.Empty,
                chamado.TecnicoResponsavelId ?? string.Empty
            };

            await _transacaoService.ExecutarAsync(async () =>
            {
                await _chamadoRepository.Remover(id);

                if (estavaNaFila)
                {
                    await _atualizacaoTempoReal
                        .NotificarFilaAtualizadaAsync(chamado.Id_Chamado);
                }

                await NotificarAtualizacaoTempoReal(
                    usuariosAfetados,
                    chamado.Id_Chamado);
            });

            // Somente depois que a transação foi confirmada.
            _centralAtendimentoTempoRealService.SinalizarAtualizacao();
        }

        // ==========================
        // LISTAGENS DE CHAMADOS
        // ==========================

        // Os nomes de solicitante e técnico vêm do serviço de usuários e não da entidade Chamado.
        // Por isso, as listagens enriquecem os DTOs somente depois do mapeamento.
        public async Task<IEnumerable<ChamadoResponseDto>> BuscarFilaAtendimento()
        {
            var chamado = await _chamadoRepository.BuscarFilaAtendimento();
            var response = _mapper.Map<IEnumerable<ChamadoResponseDto>>(chamado);

            foreach (var item in response)
            {
                if (!string.IsNullOrEmpty(item.SolicitanteId))
                {
                    var solicitante =
                        await _usuarioService.BuscarPorIdInterno(item.SolicitanteId);
                    item.SolicitanteNome = solicitante.Username;
                }
                if (!string.IsNullOrEmpty(item.TecnicoResponsavelId))
                {
                    var tecnico =
                        await _usuarioService.BuscarPorIdInterno(item.TecnicoResponsavelId);
                    item.TecnicoNome = tecnico.Username;
                }
            }
            return response;
        }

        public async Task<IEnumerable<ChamadoResponseDto>> BuscarMeusChamados(string tecnicoId)
        {
            var chamado = await _chamadoRepository.BuscarMeusChamados(tecnicoId);
            var response = _mapper.Map<IEnumerable<ChamadoResponseDto>>(chamado);

            foreach (var chamados in response)
            {
                if (!string.IsNullOrEmpty(chamados.SolicitanteId))
                {
                    var solicitante = await _usuarioService.BuscarPorIdInterno(
                        chamados.SolicitanteId

                    );

                    chamados.SolicitanteNome = solicitante.Username;
                }

                if (!string.IsNullOrEmpty(chamados.TecnicoResponsavelId))
                {
                    var tecnico  = await _usuarioService.BuscarPorIdInterno(
                        chamados.TecnicoResponsavelId
                        
                    );

                    chamados.TecnicoNome = tecnico.Username;
                }
            }
            return response;
        }

        public async Task<IEnumerable<ChamadoResponseDto>> BuscarMeusChamadosSolicitante(string solicitanteId)
        {
            var chamado = await _chamadoRepository.BuscarMeusChamadosSolicitante(solicitanteId);
            var response = _mapper.Map<IEnumerable<ChamadoResponseDto>>(chamado);

            foreach (var chamados in response)
            {
                if (!string.IsNullOrEmpty(chamados.TecnicoResponsavelId))
                {
                    var tecnico = await _usuarioService.BuscarPorIdInterno(
                        chamados.TecnicoResponsavelId
                    );

                    chamados.TecnicoNome = tecnico.Username;
                }

                if (!string.IsNullOrEmpty(chamados.SolicitanteId))
                {
                    var solicitante = await _usuarioService.BuscarPorIdInterno(
                        chamados.SolicitanteId
                    );

                    chamados.SolicitanteNome = solicitante.Username;
                }
            }
            return response;
        }

        // ==========================
        // CONSULTAS PAGINADAS
        // ==========================

        public async Task<PaginacaoResponseDto<ChamadoResponseDto>> BuscarHistoricoPaginado(
            string usuarioId,
            int perfilId,
            DateTime? dataInicial,
            DateTime? dataFinal,
            PaginacaoRequestDto request)
        {
            var resultado = await _chamadoRepository.BuscarHistoricoPaginado(
                usuarioId,
                perfilId,
                dataInicial,
                dataFinal,
                request.Pagina,
                request.TamanhoPagina);

            var chamadosDto = _mapper.Map<IEnumerable<ChamadoResponseDto>>(resultado.Chamados);

            foreach (var chamados in chamadosDto)
            {
                if (!string.IsNullOrEmpty(chamados.TecnicoResponsavelId))
                {
                    var tecnico = await _usuarioService.BuscarPorIdInterno(
                        chamados.TecnicoResponsavelId
                    );

                    chamados.TecnicoNome = tecnico.Username;
                }

                if (!string.IsNullOrEmpty(chamados.SolicitanteId))
                {
                    var solicitante = await _usuarioService.BuscarPorIdInterno(
                        chamados.SolicitanteId
                    );

                    chamados.SolicitanteNome = solicitante.Username;
                }

                chamados.PrioridadeNome = chamados.Prioridade.ToString();
            }

            return new PaginacaoResponseDto<ChamadoResponseDto>
            {
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,

                TotalPaginas = (int)Math.Ceiling(resultado.TotalRegistros / (double)request.TamanhoPagina),
                Dados = chamadosDto
            };

        }

        public async Task<PaginacaoResponseDto<ChamadoResponseDto>>
            BuscarAnalisePaginada(
                AnaliseChamadoFiltroViewModel filtro,
                PaginacaoRequestDto request)
        {
            if (filtro.DataInicial.HasValue &&
                filtro.DataFinal.HasValue &&
                filtro.DataFinal.Value.Date <
                filtro.DataInicial.Value.Date)
            {
                throw new DomainExceptionValidation(
                    "A data final não pode ser menor que a data inicial.");
            }

            if (!Enum.IsDefined(
                    typeof(TipoUsuarioDashboardEnum),
                    filtro.TipoUsuario))
            {
                throw new DomainExceptionValidation(
                    "Tipo de usuário inválido.");
            }

            if (filtro.Prioridade.HasValue &&
                !Enum.IsDefined(
                    typeof(PrioridadeEnum),
                    filtro.Prioridade.Value))
            {
                throw new DomainExceptionValidation(
                    "Prioridade inválida.");
            }

            if (filtro.Status.HasValue &&
                !Enum.IsDefined(
                    typeof(StatusChamadoEnum),
                    filtro.Status.Value))
            {
                throw new DomainExceptionValidation(
                    "Status do chamado inválido.");
            }

            if (request.Pagina <= 0)
            {
                throw new DomainExceptionValidation(
                    "A página deve ser maior que zero.");
            }

            if (request.TamanhoPagina <= 0)
            {
                throw new DomainExceptionValidation(
                    "O tamanho da página deve ser maior que zero.");
            }

            IReadOnlyCollection<string>? usuariosPerfilIds = null;

            if (filtro.PerfilId.HasValue)
            {
                usuariosPerfilIds = (
                    await _usuarioPerfilService.BuscarUsuariosPerfil(
                        filtro.PerfilId.Value))
                    .Select(usuarioPerfil => usuarioPerfil.UsuarioId)
                    .Distinct()
                    .ToArray();
            }

            var resultado = await _chamadoRepository
                .BuscarAnalisePaginada(
                    filtro.NumeroChamado,
                    filtro.DataInicial,
                    filtro.DataFinal,
                    filtro.TipoUsuario,
                    filtro.UsuarioId,
                    usuariosPerfilIds,
                    filtro.CategoriaAtendimentoId,
                    filtro.Prioridade,
                    filtro.Status,
                    request.Pagina,
                    request.TamanhoPagina);

            var chamadosDto = _mapper
                .Map<IEnumerable<ChamadoResponseDto>>(
                    resultado.Chamados)
                .ToList();

            var nomesUsuarios = new Dictionary<string, string>();

            foreach (var chamado in chamadosDto)
            {
                if (!string.IsNullOrWhiteSpace(
                        chamado.TecnicoResponsavelId))
                {
                    if (!nomesUsuarios.TryGetValue(
                            chamado.TecnicoResponsavelId,
                            out var tecnicoNome))
                    {
                        var tecnico = await _usuarioService
                            .BuscarPorIdInterno(
                                chamado.TecnicoResponsavelId);

                        tecnicoNome = tecnico.Username;
                        nomesUsuarios[chamado.TecnicoResponsavelId] =
                            tecnicoNome;
                    }

                    chamado.TecnicoNome = tecnicoNome;
                }

                if (!string.IsNullOrWhiteSpace(
                        chamado.SolicitanteId))
                {
                    if (!nomesUsuarios.TryGetValue(
                            chamado.SolicitanteId,
                            out var solicitanteNome))
                    {
                        var solicitante = await _usuarioService
                            .BuscarPorIdInterno(
                                chamado.SolicitanteId);

                        solicitanteNome = solicitante.Username;
                        nomesUsuarios[chamado.SolicitanteId] =
                            solicitanteNome;
                    }

                    chamado.SolicitanteNome = solicitanteNome;
                }

                chamado.PrioridadeNome =
                    chamado.Prioridade.ToString();
            }

            return new PaginacaoResponseDto<ChamadoResponseDto>
            {
                dataInicial = filtro.DataInicial,
                dataFinal = filtro.DataFinal,
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,
                TotalPaginas = (int)Math.Ceiling(
                    resultado.TotalRegistros /
                    (double)request.TamanhoPagina),
                Dados = chamadosDto
            };
        }

        public async Task<PaginacaoResponseDto<ChamadoResponseDto>> BuscarFilaAtendimentoPaginado(
            int prioridade,
            PaginacaoRequestDto request,
            DateTime? dataInicial,
            DateTime? dataFinal)
        {
            var resultado = await _chamadoRepository.BuscarFilaAtendimentoPaginado(
                prioridade,
                request.Pagina,
                request.TamanhoPagina,
                dataInicial,
                dataFinal);

            var chamadosDto = _mapper.Map<IEnumerable<ChamadoResponseDto>>(resultado.Chamados);

            foreach (var chamados in chamadosDto)
            {
                if (!string.IsNullOrEmpty(chamados.TecnicoResponsavelId))
                {
                    var tecnico = await _usuarioService.BuscarPorIdInterno(
                        chamados.TecnicoResponsavelId
                    );

                    chamados.TecnicoNome = tecnico.Username;
                }

                if (!string.IsNullOrEmpty(chamados.SolicitanteId))
                {
                    var solicitante = await _usuarioService.BuscarPorIdInterno(
                        chamados.SolicitanteId
                    );

                    chamados.SolicitanteNome = solicitante.Username;
                }
                chamados.PrioridadeNome = chamados.Prioridade.ToString();
            }

            return new PaginacaoResponseDto<ChamadoResponseDto>
            {
         
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,

                TotalPaginas = (int)Math.Ceiling(resultado.TotalRegistros / (double)request.TamanhoPagina),
                Dados = chamadosDto
            };
        }

        public async Task<PaginacaoResponseDto<ChamadoResponseDto>> BuscarMeusChamadosPaginado(
            bool vizualizarTodos,
            string tecnicoId,
            PaginacaoRequestDto request)
        {
            var resultado = await _chamadoRepository.BuscarMeusChamadosPaginado(
                vizualizarTodos,
                tecnicoId,
                request.Pagina,
                request.TamanhoPagina);


            var chamadosDto = _mapper.Map<IEnumerable<ChamadoResponseDto>>(resultado.Chamados);

            foreach (var chamados in chamadosDto)
            {
                if (!string.IsNullOrEmpty(chamados.SolicitanteId))
                {
                    var solicitante = await _usuarioService.BuscarPorIdInterno(
                        chamados.SolicitanteId

                    );

                    chamados.SolicitanteNome = solicitante.Username;
                }

                if (!string.IsNullOrEmpty(chamados.TecnicoResponsavelId))
                {
                    var tecnico = await _usuarioService.BuscarPorIdInterno(
                        chamados.TecnicoResponsavelId

                    );

                    chamados.TecnicoNome = tecnico.Username;
                }
                chamados.PrioridadeNome = chamados.Prioridade.ToString();
            }
                return new PaginacaoResponseDto<ChamadoResponseDto>
            {
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,

                TotalPaginas = (int)Math.Ceiling(resultado.TotalRegistros / (double)request.TamanhoPagina),
                Dados = chamadosDto
            };
        }

        public async Task<PaginacaoResponseDto<ChamadoResponseDto>> BuscarMeusChamadosSolicitantePaginado(
            bool visualizarTodos,
            string solicitanteId,
            PaginacaoRequestDto request)
        {
            var resultado = await _chamadoRepository.BuscarMeusChamadosSolicitantesPaginado(
                 visualizarTodos,
                 solicitanteId,
                 request.Pagina,
                 request.TamanhoPagina);


            var chamadosDto = _mapper.Map<IEnumerable<ChamadoResponseDto>>(resultado.Chamados);

            foreach (var chamados in chamadosDto)
            {
                if (!string.IsNullOrEmpty(chamados.TecnicoResponsavelId))
                {
                    var tecnico = await _usuarioService.BuscarPorIdInterno(
                        chamados.TecnicoResponsavelId
                    );

                    chamados.TecnicoNome = tecnico.Username;
                }

                if (!string.IsNullOrEmpty(chamados.SolicitanteId))
                {
                    var solicitante = await _usuarioService.BuscarPorIdInterno(
                        chamados.SolicitanteId
                    );

                    chamados.SolicitanteNome = solicitante.Username;
                }
                chamados.PrioridadeNome = chamados.Prioridade.ToString();
            }
            return new PaginacaoResponseDto<ChamadoResponseDto>
            {
                Pagina = request.Pagina,
                TamanhoPagina = request.TamanhoPagina,
                TotalRegistros = resultado.TotalRegistros,

                TotalPaginas = (int)Math.Ceiling(resultado.TotalRegistros / (double)request.TamanhoPagina),
                Dados = chamadosDto
            };
        }

        // ==========================
        // MÉTODOS INTERNOS
        // ==========================

        // Notifica os usuários envolvidos e atualiza os dashboards após uma alteração no chamado.
        // Funciona como uma campainha: não envia o chamado completo,
        // apenas solicita que cada tela interessada consulte novamente os dados pela API.
        private async Task NotificarAtualizacaoTempoReal(
            IEnumerable<string> usuariosAfetados,
            int chamadoId)
        {
            await _atualizacaoTempoReal.NotificarChamadoAtualizadoAsync(
                usuariosAfetados,
                chamadoId);

            await _atualizacaoTempoReal.NotificarDashboardAtualizadoAsync(
                usuariosAfetados);

            await _atualizacaoTempoReal
                .NotificarDashboardGlobalAtualizadoAsync();
        }

        // Resolve os dados do autor e registra a auditoria na própria agregação do chamado.
        // A auditoria é persistida junto com o chamado na próxima atualização do repositório.
        private async Task RegistrarAuditoriaUsuario(
            Chamado chamado,
            EventoAuditoriaChamadoEnum evento,
            string usuarioId,
            StatusChamadoEnum? statusAnterior,
            StatusChamadoEnum? statusNovo,
            string descricao,
            string? detalhesAnteriores = null,
            string? detalhesNovos = null)
        {
            var usuario = await _usuarioService.BuscarPorIdInterno(usuarioId);
            var perfil = await ObterNomePerfil(usuario.Id_Perfil);

            chamado.RegistrarAuditoria(
                evento,
                usuario.Id,
                usuario.Username,
                perfil,
                statusAnterior,
                statusNovo,
                descricao,
                detalhesAnteriores,
                detalhesNovos);
        }

        // Retorna o nome usado no histórico sem obrigar fluxos sem perfil a criar um valor artificial.
        private async Task<string?> ObterNomePerfil(int? perfilId)
        {
            if (!perfilId.HasValue)
                return null;

            var perfil = await _perfilService.BuscarPorId(perfilId.Value);
            return perfil?.Nome;
        }
    }
}
