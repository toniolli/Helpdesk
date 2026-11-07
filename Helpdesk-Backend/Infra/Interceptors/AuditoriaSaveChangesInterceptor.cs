using Domain.Entities;
using Domain.Enum;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using System.Diagnostics;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Infra.Interceptors
{
    public sealed class AuditoriaSaveChangesInterceptor
        : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor
            _httpContextAccessor;

        private bool _salvandoAuditoria;

        private IDbContextTransaction?
            _transacaoCriada;

        private List<AlteracaoPendente>
            _alteracoesPendentes = new();

        private static readonly JsonSerializerOptions
            JsonOptions = new()
            {
                Converters =
                {
                    new JsonStringEnumConverter()
                }
            };

        public AuditoriaSaveChangesInterceptor(
            IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor =
                httpContextAccessor;
        }

        public override InterceptionResult<int>
            SavingChanges(
                DbContextEventData eventData,
                InterceptionResult<int> result)
        {
            if (!_salvandoAuditoria)
            {
                PrepararAuditoria(
                    eventData.Context);

                IniciarTransacao(
                    eventData.Context);
            }

            return base.SavingChanges(
                eventData,
                result);
        }

        public override async
            ValueTask<InterceptionResult<int>>
            SavingChangesAsync(
                DbContextEventData eventData,
                InterceptionResult<int> result,
                CancellationToken cancellationToken =
                    default)
        {
            if (!_salvandoAuditoria)
            {
                PrepararAuditoria(
                    eventData.Context);

                await IniciarTransacaoAsync(
                    eventData.Context,
                    cancellationToken);
            }

            return await base.SavingChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        public override int SavedChanges(
            SaveChangesCompletedEventData eventData,
            int result)
        {
            if (!_salvandoAuditoria)
            {
                SalvarAuditorias(
                    eventData.Context);
            }

            return base.SavedChanges(
                eventData,
                result);
        }

        public override async ValueTask<int>
            SavedChangesAsync(
                SaveChangesCompletedEventData eventData,
                int result,
                CancellationToken cancellationToken =
                    default)
        {
            if (!_salvandoAuditoria)
            {
                await SalvarAuditoriasAsync(
                    eventData.Context,
                    cancellationToken);
            }

            return await base.SavedChangesAsync(
                eventData,
                result,
                cancellationToken);
        }

        public override void SaveChangesFailed(
            DbContextErrorEventData eventData)
        {
            DesfazerTransacao();
            LimparEstado();

            base.SaveChangesFailed(eventData);
        }

        public override async Task
            SaveChangesFailedAsync(
                DbContextErrorEventData eventData,
                CancellationToken cancellationToken =
                    default)
        {
            await DesfazerTransacaoAsync();
            LimparEstado();

            await base.SaveChangesFailedAsync(
                eventData,
                cancellationToken);
        }

        public override void SaveChangesCanceled(
            DbContextEventData eventData)
        {
            DesfazerTransacao();
            LimparEstado();

            base.SaveChangesCanceled(eventData);
        }

        public override async Task
            SaveChangesCanceledAsync(
                DbContextEventData eventData,
                CancellationToken cancellationToken =
                    default)
        {
            await DesfazerTransacaoAsync();
            LimparEstado();

            await base.SaveChangesCanceledAsync(
                eventData,
                cancellationToken);
        }

        private void PrepararAuditoria(
            DbContext? context)
        {
            _alteracoesPendentes.Clear();

            if (context == null)
                return;

            context.ChangeTracker.DetectChanges();

            _alteracoesPendentes =
                CapturarAlteracoes(context);
        }

        private void IniciarTransacao(
            DbContext? context)
        {
            if (context == null ||
                _alteracoesPendentes.Count == 0 ||
                context.Database.CurrentTransaction != null)
            {
                return;
            }

            _transacaoCriada =
                context.Database.BeginTransaction();
        }

        private async Task IniciarTransacaoAsync(
            DbContext? context,
            CancellationToken cancellationToken)
        {
            if (context == null ||
                _alteracoesPendentes.Count == 0 ||
                context.Database.CurrentTransaction != null)
            {
                return;
            }

            _transacaoCriada =
                await context.Database
                    .BeginTransactionAsync(
                        cancellationToken);
        }

        private void SalvarAuditorias(
            DbContext? context)
        {
            if (context == null ||
                _alteracoesPendentes.Count == 0)
            {
                ConfirmarTransacao();
                LimparEstado();
                return;
            }

            _salvandoAuditoria = true;

            try
            {
                var auditorias =
                    CriarAuditorias();

                context.Set<Auditoria>()
                    .AddRange(auditorias);

                context.SaveChanges();

                ConfirmarTransacao();
            }
            catch
            {
                DesfazerTransacao();
                throw;
            }
            finally
            {
                LimparEstado();
            }
        }

        private async Task SalvarAuditoriasAsync(
            DbContext? context,
            CancellationToken cancellationToken)
        {
            if (context == null ||
                _alteracoesPendentes.Count == 0)
            {
                await ConfirmarTransacaoAsync();
                LimparEstado();
                return;
            }

            _salvandoAuditoria = true;

            try
            {
                var auditorias =
                    CriarAuditorias();

                await context.Set<Auditoria>()
                    .AddRangeAsync(
                        auditorias,
                        cancellationToken);

                await context.SaveChangesAsync(
                    cancellationToken);

                await ConfirmarTransacaoAsync();
            }
            catch
            {
                await DesfazerTransacaoAsync();
                throw;
            }
            finally
            {
                LimparEstado();
            }
        }

        private static List<AlteracaoPendente>
            CapturarAlteracoes(
                DbContext context)
        {
            var alteracoes =
                new List<AlteracaoPendente>();

            var entradas = context.ChangeTracker
                .Entries()
                .Where(entry =>
                    entry.State == EntityState.Added ||
                    entry.State == EntityState.Modified ||
                    entry.State == EntityState.Deleted)
                .Where(entry =>
                    DeveAuditar(entry.Entity))
                .ToList();

            foreach (var entry in entradas)
            {
                var alteracao =
                    new AlteracaoPendente(
                        entry,
                        entry.State,
                        entry.Metadata.ClrType.Name,
                        ObterAcao(entry));

                foreach (var propriedade
                    in entry.Properties)
                {
                    var nome =
                        propriedade.Metadata.Name;

                    if (PropriedadeSensivel(nome))
                        continue;

                    if (entry.State ==
                        EntityState.Deleted)
                    {
                        alteracao
                            .ValoresAnteriores[nome] =
                            propriedade.OriginalValue;
                    }

                    if (entry.State ==
                            EntityState.Modified &&
                        propriedade.IsModified &&
                        !Equals(
                            propriedade.OriginalValue,
                            propriedade.CurrentValue))
                    {
                        alteracao
                            .ValoresAnteriores[nome] =
                            propriedade.OriginalValue;

                        alteracao
                            .ValoresAtuais[nome] =
                            propriedade.CurrentValue;
                    }
                }

                if (entry.State != EntityState.Added)
                {
                    alteracao.RegistroId =
                        ObterRegistroId(entry);
                }

                if (entry.State ==
                        EntityState.Modified &&
                    alteracao.ValoresAtuais.Count == 0)
                {
                    continue;
                }

                alteracoes.Add(alteracao);
            }

            return alteracoes;
        }

        private List<Auditoria>
            CriarAuditorias()
        {
            var httpContext =
                _httpContextAccessor.HttpContext;

            var usuarioId = httpContext?
                .User?
                .FindFirst(
                    ClaimTypes.NameIdentifier)?
                .Value;

            var usuarioNome = httpContext?
                .User?
                .FindFirst(ClaimTypes.Name)?
                .Value;

            var origem =
                string.IsNullOrWhiteSpace(usuarioId)
                    ? OrigemAuditoriaEnum.Sistema
                    : OrigemAuditoriaEnum.Usuario;

            if (string.IsNullOrWhiteSpace(
                usuarioNome))
            {
                usuarioNome =
                    origem ==
                    OrigemAuditoriaEnum.Sistema
                        ? "SISTEMA"
                        : "USUARIO_DESCONHECIDO";
            }

            var traceId =
                httpContext?.TraceIdentifier;

            if (string.IsNullOrWhiteSpace(
                traceId))
            {
                traceId = Activity.Current?
                    .TraceId
                    .ToString();
            }

            if (string.IsNullOrWhiteSpace(
                traceId))
            {
                traceId =
                    Guid.NewGuid().ToString("N");
            }

            var auditorias =
                new List<Auditoria>();

            foreach (var alteracao
                in _alteracoesPendentes)
            {
                if (alteracao.Estado ==
                    EntityState.Added)
                {
                    PreencherValoresCriados(
                        alteracao);
                }

                var registroId =
                    alteracao.RegistroId ??
                    ObterRegistroId(
                        alteracao.Entry);

                auditorias.Add(
                    new Auditoria(
                        usuarioId,
                        usuarioNome,
                        alteracao.Acao,
                        origem,
                        alteracao.Entidade,
                        registroId,
                        Serializar(
                            alteracao
                                .ValoresAnteriores),
                        Serializar(
                            alteracao
                                .ValoresAtuais),
                        httpContext?
                            .Connection
                            .RemoteIpAddress?
                            .ToString(),
                        httpContext?
                            .Request
                            .Method,
                        httpContext?
                            .Request
                            .Path
                            .Value,
                        traceId));
            }

            return auditorias;
        }

        private static void
            PreencherValoresCriados(
                AlteracaoPendente alteracao)
        {
            alteracao.ValoresAtuais.Clear();

            foreach (var propriedade
                in alteracao.Entry.Properties)
            {
                var nome =
                    propriedade.Metadata.Name;

                if (PropriedadeSensivel(nome))
                    continue;

                alteracao.ValoresAtuais[nome] =
                    propriedade.CurrentValue;
            }
        }

        private static AcaoAuditoriaEnum
            ObterAcao(
                EntityEntry entry)
        {
            var entidadeDeVinculo =
                entry.Entity is PerfilPermissao ||
                entry.Entity is UsuarioPerfil;

            if (entry.State == EntityState.Added)
            {
                return entidadeDeVinculo
                    ? AcaoAuditoriaEnum.Vinculo
                    : AcaoAuditoriaEnum.Criacao;
            }

            if (entry.State == EntityState.Deleted)
            {
                return entidadeDeVinculo
                    ? AcaoAuditoriaEnum.Desvinculo
                    : AcaoAuditoriaEnum.Exclusao;
            }

            var propriedadeAtivo =
                entry.Properties.FirstOrDefault(
                    propriedade =>
                        propriedade.Metadata.Name
                            .Equals(
                                "Ativo",
                                StringComparison
                                    .OrdinalIgnoreCase) &&
                        propriedade.IsModified);

            if (propriedadeAtivo != null &&
                propriedadeAtivo.CurrentValue
                    is bool ativoAtual &&
                propriedadeAtivo.OriginalValue
                    is bool ativoAnterior &&
                ativoAtual != ativoAnterior)
            {
                return ativoAtual
                    ? AcaoAuditoriaEnum.Ativacao
                    : AcaoAuditoriaEnum.Desativacao;
            }

            return AcaoAuditoriaEnum.Alteracao;
        }

        private static string ObterRegistroId(
            EntityEntry entry)
        {
            var chave =
                entry.Metadata.FindPrimaryKey();

            if (chave == null)
                return "SEM_CHAVE";

            var valores = chave.Properties
                .Select(propriedade =>
                    entry.Property(propriedade.Name)
                        .CurrentValue?
                        .ToString() ?? "NULL");

            return string.Join("|", valores);
        }

        private static bool DeveAuditar(
            object entidade)
        {
            return entidade is not Auditoria &&
                   entidade is not AuditoriaIdentity &&
                   entidade is not AuditoriaChamado &&
                   entidade is not Notificacao &&
                   entidade is not EmailNotification && 
                   entidade is not EventoSignalROutbox;
        }

        private static bool PropriedadeSensivel(
            string nome)
        {
            return nome.Contains(
                       "Password",
                       StringComparison.OrdinalIgnoreCase) ||
                   nome.Contains(
                       "Senha",
                       StringComparison.OrdinalIgnoreCase) ||
                   nome.Contains(
                       "Token",
                       StringComparison.OrdinalIgnoreCase) ||
                   nome.Contains(
                       "Hash",
                       StringComparison.OrdinalIgnoreCase) ||
                   nome.Contains(
                       "SecurityStamp",
                       StringComparison.OrdinalIgnoreCase) ||
                   nome.Contains(
                       "ConcurrencyStamp",
                       StringComparison.OrdinalIgnoreCase) ||
                   nome.Contains(
                       "Secret",
                       StringComparison.OrdinalIgnoreCase);
        }

        private static string? Serializar(
            Dictionary<string, object?> valores)
        {
            return valores.Count == 0
                ? null
                : JsonSerializer.Serialize(
                    valores,
                    JsonOptions);
        }

        private void ConfirmarTransacao()
        {
            if (_transacaoCriada == null)
                return;

            try
            {
                _transacaoCriada.Commit();
            }
            finally
            {
                _transacaoCriada.Dispose();
                _transacaoCriada = null;
            }
        }

        private async Task
            ConfirmarTransacaoAsync()
        {
            if (_transacaoCriada == null)
                return;

            try
            {
                await _transacaoCriada
                    .CommitAsync();
            }
            finally
            {
                await _transacaoCriada
                    .DisposeAsync();

                _transacaoCriada = null;
            }
        }

        private void DesfazerTransacao()
        {
            if (_transacaoCriada == null)
                return;

            try
            {
                _transacaoCriada.Rollback();
            }
            finally
            {
                _transacaoCriada.Dispose();
                _transacaoCriada = null;
            }
        }

        private async Task
            DesfazerTransacaoAsync()
        {
            if (_transacaoCriada == null)
                return;

            try
            {
                await _transacaoCriada
                    .RollbackAsync(
                        CancellationToken.None);
            }
            finally
            {
                await _transacaoCriada
                    .DisposeAsync();

                _transacaoCriada = null;
            }
        }

        private void LimparEstado()
        {
            _alteracoesPendentes.Clear();
            _salvandoAuditoria = false;
        }

        private sealed class AlteracaoPendente
        {
            public EntityEntry Entry { get; }
            public EntityState Estado { get; }
            public string Entidade { get; }
            public AcaoAuditoriaEnum Acao { get; }

            public string? RegistroId { get; set; }

            public Dictionary<string, object?>
                ValoresAnteriores
            { get; } =
                    new();

            public Dictionary<string, object?>
                ValoresAtuais
            { get; } =
                    new();

            public AlteracaoPendente(
                EntityEntry entry,
                EntityState estado,
                string entidade,
                AcaoAuditoriaEnum acao)
            {
                Entry = entry;
                Estado = estado;
                Entidade = entidade;
                Acao = acao;
            }
        }
    }
}
