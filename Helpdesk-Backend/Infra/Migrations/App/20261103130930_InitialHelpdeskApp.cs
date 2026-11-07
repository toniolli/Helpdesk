using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infra.Migrations.App
{
    /// <inheritdoc />
    public partial class InitialHelpdeskApp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "HELPDESK");

            migrationBuilder.CreateTable(
                name: "auditoria",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Auditoria = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UsuarioId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: true),
                    UsuarioNome = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    Acao = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Origem = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Entidade = table.Column<string>(type: "VARCHAR2(150)", unicode: false, maxLength: 150, nullable: false),
                    RegistroId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true),
                    ValoresAtuais = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true),
                    EnderecoIp = table.Column<string>(type: "VARCHAR2(45)", unicode: false, maxLength: 45, nullable: true),
                    MetodoHttp = table.Column<string>(type: "VARCHAR2(10)", unicode: false, maxLength: 10, nullable: true),
                    Rota = table.Column<string>(type: "VARCHAR2(500)", unicode: false, maxLength: 500, nullable: true),
                    TraceId = table.Column<string>(type: "VARCHAR2(200)", unicode: false, maxLength: 200, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria", x => x.Id_Auditoria);
                });

            migrationBuilder.CreateTable(
                name: "auditoria_identity",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_AuditoriaIdentity = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UsuarioId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: true),
                    UsuarioNome = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    Evento = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Origem = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Entidade = table.Column<string>(type: "VARCHAR2(150)", unicode: false, maxLength: 150, nullable: false),
                    RegistroId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: false),
                    Sucesso = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    Motivo = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    ValoresAnteriores = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true),
                    ValoresAtuais = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true),
                    EnderecoIp = table.Column<string>(type: "VARCHAR2(45)", unicode: false, maxLength: 45, nullable: true),
                    MetodoHttp = table.Column<string>(type: "VARCHAR2(10)", unicode: false, maxLength: 10, nullable: true),
                    Rota = table.Column<string>(type: "VARCHAR2(500)", unicode: false, maxLength: 500, nullable: true),
                    UserAgent = table.Column<string>(type: "VARCHAR2(1000)", unicode: false, maxLength: 1000, nullable: true),
                    TraceId = table.Column<string>(type: "VARCHAR2(200)", unicode: false, maxLength: 200, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_identity", x => x.Id_AuditoriaIdentity);
                });

            migrationBuilder.CreateTable(
                name: "categoria_atendimento",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_CategoriaAtendimento = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: true),
                    Prioridade = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Ativo = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_atendimento", x => x.Id_CategoriaAtendimento);
                });

            migrationBuilder.CreateTable(
                name: "eventosignalroutbox",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_EventoSignalROutbox = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NomeEvento = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: false),
                    TipoDestino = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DestinatariosJson = table.Column<string>(type: "CLOB", nullable: true),
                    GrupoDestino = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: true),
                    PayloadJson = table.Column<string>(type: "CLOB", nullable: true),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 1),
                    QuantidadeTentativas = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 0),
                    DataCriacaoUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ProximaTentativaUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    ProcessamentoIniciadoUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DataEnvioUtc = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UltimoErro = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventosignalroutbox", x => x.Id_EventoSignalROutbox);
                });

            migrationBuilder.CreateTable(
                name: "perfil",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Perfil = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: true),
                    Ativo = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfil", x => x.Id_Perfil);
                });

            migrationBuilder.CreateTable(
                name: "permissao",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Permissao = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: true),
                    Ativo = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissao", x => x.Id_Permissao);
                });

            migrationBuilder.CreateTable(
                name: "regra_distribuicao",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Regras = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Nome = table.Column<string>(type: "VARCHAR2(150)", unicode: false, maxLength: 150, nullable: false),
                    TipoRegra = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TipoDestino = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Ativo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    PeriodoIndefinido = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    DataInicial = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DataFinal = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    UsuarioDestinoId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: true),
                    PerfilDestinoId = table.Column<int>(type: "NUMBER(10)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_regra_distribuicao", x => x.Id_Regras);
                });

            migrationBuilder.CreateTable(
                name: "sla",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_SLA = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Prioridade = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TempoAtendimentoHoras = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TempoResolucaoHoras = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Ativo = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sla", x => x.Id_SLA);
                });

            migrationBuilder.CreateTable(
                name: "periodo_trabalho_perfil",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_PeriodoTrabalhoPerfil = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    PerfilId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Versao = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DataInicioVigencia = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataFimVigencia = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    SegundaAtivo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    TercaAtivo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    QuartaAtivo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    QuintaAtivo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    SextaAtivo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    HoraInicioSemana = table.Column<short>(type: "NUMBER(5)", nullable: true),
                    HoraFimSemana = table.Column<short>(type: "NUMBER(5)", nullable: true),
                    SabadoAtivo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    HoraInicioSabado = table.Column<short>(type: "NUMBER(5)", nullable: true),
                    HoraFimSabado = table.Column<short>(type: "NUMBER(5)", nullable: true),
                    DomingoAtivo = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    HoraInicioDomingo = table.Column<short>(type: "NUMBER(5)", nullable: true),
                    HoraFimDomingo = table.Column<short>(type: "NUMBER(5)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodo_trabalho_perfil", x => x.Id_PeriodoTrabalhoPerfil);
                    table.ForeignKey(
                        name: "FK_periodo_trabalho_perfil_perfil_PerfilId",
                        column: x => x.PerfilId,
                        principalSchema: "HELPDESK",
                        principalTable: "perfil",
                        principalColumn: "Id_Perfil",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuario_perfil",
                schema: "HELPDESK",
                columns: table => new
                {
                    UsuarioId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: false),
                    PerfilId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_perfil", x => new { x.UsuarioId, x.PerfilId });
                    table.ForeignKey(
                        name: "FK_usuario_perfil_perfil_PerfilId",
                        column: x => x.PerfilId,
                        principalSchema: "HELPDESK",
                        principalTable: "perfil",
                        principalColumn: "Id_Perfil",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "perfil_permissao",
                schema: "HELPDESK",
                columns: table => new
                {
                    PerfilId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    PermissaoId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_perfil_permissao", x => new { x.PerfilId, x.PermissaoId });
                    table.ForeignKey(
                        name: "FK_perfil_permissao_perfil_PerfilId",
                        column: x => x.PerfilId,
                        principalSchema: "HELPDESK",
                        principalTable: "perfil",
                        principalColumn: "Id_Perfil",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_perfil_permissao_permissao_PermissaoId",
                        column: x => x.PermissaoId,
                        principalSchema: "HELPDESK",
                        principalTable: "permissao",
                        principalColumn: "Id_Permissao",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "criterio_distribuicao",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    RegraDistribuicaoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    CategoriaAtendimentoId = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_criterio_distribuicao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_criterio_distribuicao_categoria_atendimento_CategoriaAtendimentoId",
                        column: x => x.CategoriaAtendimentoId,
                        principalSchema: "HELPDESK",
                        principalTable: "categoria_atendimento",
                        principalColumn: "Id_CategoriaAtendimento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_criterio_distribuicao_regra_distribuicao_RegraDistribuicaoId",
                        column: x => x.RegraDistribuicaoId,
                        principalSchema: "HELPDESK",
                        principalTable: "regra_distribuicao",
                        principalColumn: "Id_Regras",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "chamado",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Chamado = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    Numero = table.Column<string>(type: "VARCHAR2(30)", unicode: false, maxLength: 30, nullable: true),
                    SolicitanteId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: true),
                    CategoriaAtendimentoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EquipamentoAfetado = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    Descricao = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: false),
                    Prioridade = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    TecnicoResponsavelId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: true),
                    TipoAtendimento = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TipoEncerramento = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Solucao = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true),
                    Version = table.Column<int>(type: "NUMBER(10)", nullable: false, defaultValue: 1),
                    DataAbertura = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataPrimeiroAtendimento = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    DataInicioPausaSla = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    TempoPausadoHoras = table.Column<double>(type: "BINARY_DOUBLE", nullable: false, defaultValue: 0.0),
                    DataFechamento = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    TempoPausadoEquipamentoHoras = table.Column<double>(type: "BINARY_DOUBLE", nullable: false, defaultValue: 0.0),
                    PeriodoTrabalhoPerfilId = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    TempoPausadoSlaRetornoHoras = table.Column<double>(type: "BINARY_DOUBLE", nullable: false, defaultValue: 0.0),
                    TempoPausadoSlaEquipamentoHoras = table.Column<double>(type: "BINARY_DOUBLE", nullable: false, defaultValue: 0.0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_chamado", x => x.Id_Chamado);
                    table.ForeignKey(
                        name: "FK_chamado_categoria_atendimento_CategoriaAtendimentoId",
                        column: x => x.CategoriaAtendimentoId,
                        principalSchema: "HELPDESK",
                        principalTable: "categoria_atendimento",
                        principalColumn: "Id_CategoriaAtendimento",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_chamado_periodo_trabalho_perfil_PeriodoTrabalhoPerfilId",
                        column: x => x.PeriodoTrabalhoPerfilId,
                        principalSchema: "HELPDESK",
                        principalTable: "periodo_trabalho_perfil",
                        principalColumn: "Id_PeriodoTrabalhoPerfil",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "auditoria_chamado",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_AuditoriaChamado = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ChamadoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NumeroChamado = table.Column<string>(type: "VARCHAR2(30)", unicode: false, maxLength: 30, nullable: false),
                    Evento = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DataEvento = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    UsuarioId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: true),
                    UsuarioNome = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    PerfilUsuario = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: true),
                    StatusAnterior = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    StatusNovo = table.Column<int>(type: "NUMBER(10)", nullable: true),
                    Descricao = table.Column<string>(type: "VARCHAR2(1000)", unicode: false, maxLength: 1000, nullable: false),
                    DetalhesAnteriores = table.Column<string>(type: "CLOB", nullable: true),
                    DetalhesNovos = table.Column<string>(type: "CLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auditoria_chamado", x => x.Id_AuditoriaChamado);
                    table.ForeignKey(
                        name: "FK_auditoria_chamado_chamado_ChamadoId",
                        column: x => x.ChamadoId,
                        principalSchema: "HELPDESK",
                        principalTable: "chamado",
                        principalColumn: "Id_Chamado",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "comentario",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Comentario = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ChamadoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    UsuarioId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: false),
                    Mensagem = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comentario", x => x.Id_Comentario);
                    table.ForeignKey(
                        name: "FK_comentario_chamado_ChamadoId",
                        column: x => x.ChamadoId,
                        principalSchema: "HELPDESK",
                        principalTable: "chamado",
                        principalColumn: "Id_Chamado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "emailnotification",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_EmailNotfication = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ChamadoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tipo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Destinatario = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    Assunto = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    QuantidadeTentativas = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    MensagemErro = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_emailnotification", x => x.Id_EmailNotfication);
                    table.ForeignKey(
                        name: "FK_emailnotification_chamado_ChamadoId",
                        column: x => x.ChamadoId,
                        principalSchema: "HELPDESK",
                        principalTable: "chamado",
                        principalColumn: "Id_Chamado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notificacao",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Notificacao = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    UsuarioId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: false),
                    UsuarioOrigemId = table.Column<string>(type: "VARCHAR2(450)", unicode: false, maxLength: 450, nullable: true),
                    ChamadoId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Tipo = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    Titulo = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    Mensagem = table.Column<string>(type: "VARCHAR2(4000)", unicode: false, maxLength: 4000, nullable: false),
                    Lida = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    Status = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DataLida = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacao", x => x.Id_Notificacao);
                    table.ForeignKey(
                        name: "FK_notificacao_chamado_ChamadoId",
                        column: x => x.ChamadoId,
                        principalSchema: "HELPDESK",
                        principalTable: "chamado",
                        principalColumn: "Id_Chamado",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "anexo",
                schema: "HELPDESK",
                columns: table => new
                {
                    Id_Anexo = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    ComentarioId = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    NomeArquivo = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    NomeOriginal = table.Column<string>(type: "VARCHAR2(255)", unicode: false, maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "VARCHAR2(100)", unicode: false, maxLength: 100, nullable: false),
                    Tamanho = table.Column<long>(type: "NUMBER(19)", nullable: false),
                    CaminhoArquivo = table.Column<string>(type: "VARCHAR2(500)", unicode: false, maxLength: 500, nullable: false),
                    DataUpload = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_anexo", x => x.Id_Anexo);
                    table.ForeignKey(
                        name: "FK_anexo_comentario_ComentarioId",
                        column: x => x.ComentarioId,
                        principalSchema: "HELPDESK",
                        principalTable: "comentario",
                        principalColumn: "Id_Comentario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "HELPDESK",
                table: "permissao",
                columns: new[] { "Id_Permissao", "Ativo", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, true, "Permite abrir chamados.", "ABRIR_CHAMADO" },
                    { 2, true, "Permite anexar arquivos aos chamados.", "ANEXAR_ARQUIVOS" },
                    { 3, true, "Permite assumir chamados para atendimento.", "ASSUMIR_CHAMADO" },
                    { 4, true, "Permite adicionar comentários em chamados.", "COMENTAR_CHAMADO" },
                    { 5, true, "Permite configurar os SLAs.", "CONFIGURAR_SLA" },
                    { 6, true, "Permite criar notificações.", "CRIAR_NOTIFICACAO" },
                    { 7, true, "Permite encerrar chamados.", "ENCERRAR_CHAMADO" },
                    { 8, true, "Permite exportar o histórico de chamados.", "EXPORTAR_HISTORICO" },
                    { 9, true, "Permite gerenciar categorias de atendimento.", "GERENCIAR_CATEGORIAS" },
                    { 10, true, "Permite gerenciar regras de distribuição.", "GERENCIAR_DISTRIBUICAO" },
                    { 11, true, "Permite gerenciar perfis.", "GERENCIAR_PERFIS" },
                    { 12, true, "Permite gerenciar permissões.", "GERENCIAR_PERMISSOES" },
                    { 13, true, "Permite gerenciar usuários.", "GERENCIAR_USUARIOS" },
                    { 14, true, "Permite visualizar auditorias e monitoramento.", "VISUALIZAR_AUDITORIA" },
                    { 15, true, "Permite visualizar o dashboard.", "VISUALIZAR_DASHBOARD" },
                    { 16, true, "Permite visualizar a fila de atendimento.", "VISUALIZAR_FILA" },
                    { 17, true, "Permite visualizar o histórico de chamados.", "VISUALIZAR_HISTORICO" },
                    { 18, true, "Permite visualizar notificações.", "VISUALIZAR_NOTIFICACAO" },
                    { 19, true, "Permite visualizar os SLAs.", "VISUALIZAR_SLA" },
                    { 20, true, "Permite visualizar todos os chamados.", "VISUALIZAR_TODOS_CHAMADOS" },
                    { 21, true, "Permite gerenciar categorias de atendimento.", "VISUALIZAR_CATEGORIAS" },
                    { 22, true, "Permite visualizar o dashboard de administrador.", "DASHBOARD_ADMIN" },
                    { 23, true, "Permite visualizar o dashboard de técnico.", "DASHBOARD_TECNICO" },
                    { 24, true, "Permite visualizar o dashboard de solicitante.", "DASHBOARD_SOLICITANTE" },
                    { 25, true, "Permite visualizar a auditoria de chamados.", "VISUALIZAR_AUDITORIA_CHAMADO" },
                    { 26, true, "Permite visualizar a Central de Atendimento.", "VISUALIZAR_CENTRAL_ATENDIMENTO" }
                });

            migrationBuilder.InsertData(
                schema: "HELPDESK",
                table: "sla",
                columns: new[] { "Id_SLA", "Ativo", "Prioridade", "TempoAtendimentoHoras", "TempoResolucaoHoras" },
                values: new object[,]
                {
                    { 1, true, 1, 8, 72 },
                    { 2, true, 2, 4, 48 },
                    { 3, true, 3, 2, 24 },
                    { 4, true, 4, 1, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_anexo_ComentarioId",
                schema: "HELPDESK",
                table: "anexo",
                column: "ComentarioId");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_Acao_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria",
                columns: new[] { "Acao", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_Entidade_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria",
                columns: new[] { "Entidade", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_UsuarioId_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria",
                columns: new[] { "UsuarioId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_chamado_ChamadoId_DataEvento",
                schema: "HELPDESK",
                table: "auditoria_chamado",
                columns: new[] { "ChamadoId", "DataEvento" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_chamado_DataEvento",
                schema: "HELPDESK",
                table: "auditoria_chamado",
                column: "DataEvento");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_chamado_Evento_DataEvento",
                schema: "HELPDESK",
                table: "auditoria_chamado",
                columns: new[] { "Evento", "DataEvento" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_chamado_UsuarioId_DataEvento",
                schema: "HELPDESK",
                table: "auditoria_chamado",
                columns: new[] { "UsuarioId", "DataEvento" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_identity_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria_identity",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_identity_Evento_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria_identity",
                columns: new[] { "Evento", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_identity_Sucesso_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria_identity",
                columns: new[] { "Sucesso", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_auditoria_identity_UsuarioId_DataCriacao",
                schema: "HELPDESK",
                table: "auditoria_identity",
                columns: new[] { "UsuarioId", "DataCriacao" });

            migrationBuilder.CreateIndex(
                name: "IX_chamado_CategoriaAtendimentoId",
                schema: "HELPDESK",
                table: "chamado",
                column: "CategoriaAtendimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_chamado_PeriodoTrabalhoPerfilId",
                schema: "HELPDESK",
                table: "chamado",
                column: "PeriodoTrabalhoPerfilId");

            migrationBuilder.CreateIndex(
                name: "IX_comentario_ChamadoId",
                schema: "HELPDESK",
                table: "comentario",
                column: "ChamadoId");

            migrationBuilder.CreateIndex(
                name: "IX_criterio_distribuicao_CategoriaAtendimentoId",
                schema: "HELPDESK",
                table: "criterio_distribuicao",
                column: "CategoriaAtendimentoId");

            migrationBuilder.CreateIndex(
                name: "IX_criterio_distribuicao_RegraDistribuicaoId",
                schema: "HELPDESK",
                table: "criterio_distribuicao",
                column: "RegraDistribuicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_emailnotification_ChamadoId",
                schema: "HELPDESK",
                table: "emailnotification",
                column: "ChamadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_Status_DataEnvio",
                schema: "HELPDESK",
                table: "eventosignalroutbox",
                columns: new[] { "Status", "DataEnvioUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Outbox_Status_ProximaTentativa",
                schema: "HELPDESK",
                table: "eventosignalroutbox",
                columns: new[] { "Status", "ProximaTentativaUtc", "DataCriacaoUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_notificacao_ChamadoId",
                schema: "HELPDESK",
                table: "notificacao",
                column: "ChamadoId");

            migrationBuilder.CreateIndex(
                name: "IX_perfil_permissao_PermissaoId",
                schema: "HELPDESK",
                table: "perfil_permissao",
                column: "PermissaoId");

            migrationBuilder.CreateIndex(
                name: "IX_periodo_trabalho_perfil_PerfilId_Versao",
                schema: "HELPDESK",
                table: "periodo_trabalho_perfil",
                columns: new[] { "PerfilId", "Versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuario_perfil_PerfilId",
                schema: "HELPDESK",
                table: "usuario_perfil",
                column: "PerfilId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "anexo",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "auditoria",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "auditoria_chamado",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "auditoria_identity",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "criterio_distribuicao",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "emailnotification",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "eventosignalroutbox",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "notificacao",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "perfil_permissao",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "sla",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "usuario_perfil",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "comentario",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "regra_distribuicao",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "permissao",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "chamado",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "categoria_atendimento",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "periodo_trabalho_perfil",
                schema: "HELPDESK");

            migrationBuilder.DropTable(
                name: "perfil",
                schema: "HELPDESK");
        }
    }
}
