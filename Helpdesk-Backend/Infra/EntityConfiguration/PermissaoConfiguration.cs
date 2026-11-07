using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infra.EntityConfiguration
{
    public class PermissaoConfiguration : IEntityTypeConfiguration<Permissao>
    {
        public void Configure(
            EntityTypeBuilder<Permissao> builder)
        {
            builder.ToTable("permissao");

            // PK
            builder.HasKey(p => p.Id_Permissao);

            builder.Property(p => p.Id_Permissao)
                .HasColumnName("Id_Permissao")
                .ValueGeneratedOnAdd();

            // Propriedades
            builder.Property(p => p.Nome)
                .HasColumnName("Nome")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(p => p.Descricao)
                .HasColumnName("Descricao")
                .HasMaxLength(255);

            builder.Property(p => p.Ativo)
                .HasColumnName("Ativo")
                .HasColumnType("NUMBER(1)")
                .IsRequired();

            // Relação Permissao 1:N PerfilPermissao
            builder.HasMany(p => p.PerfisPermissoes)
                .WithOne(pp => pp.Permissao)
                .HasForeignKey(pp => pp.PermissaoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasData(
                new
                {
                    Id_Permissao = 1,
                    Nome = "ABRIR_CHAMADO",
                    Descricao = "Permite abrir chamados.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 2,
                    Nome = "ANEXAR_ARQUIVOS",
                    Descricao = "Permite anexar arquivos aos chamados.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 3,
                    Nome = "ASSUMIR_CHAMADO",
                    Descricao = "Permite assumir chamados para atendimento.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 4,
                    Nome = "COMENTAR_CHAMADO",
                    Descricao = "Permite adicionar comentários em chamados.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 5,
                    Nome = "CONFIGURAR_SLA",
                    Descricao = "Permite configurar os SLAs.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 6,
                    Nome = "CRIAR_NOTIFICACAO",
                    Descricao = "Permite criar notificações.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 7,
                    Nome = "ENCERRAR_CHAMADO",
                    Descricao = "Permite encerrar chamados.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 8,
                    Nome = "EXPORTAR_HISTORICO",
                    Descricao = "Permite exportar o histórico de chamados.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 9,
                    Nome = "GERENCIAR_CATEGORIAS",
                    Descricao = "Permite gerenciar categorias de atendimento.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 10,
                    Nome = "GERENCIAR_DISTRIBUICAO",
                    Descricao = "Permite gerenciar regras de distribuição.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 11,
                    Nome = "GERENCIAR_PERFIS",
                    Descricao = "Permite gerenciar perfis.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 12,
                    Nome = "GERENCIAR_PERMISSOES",
                    Descricao = "Permite gerenciar permissões.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 13,
                    Nome = "GERENCIAR_USUARIOS",
                    Descricao = "Permite gerenciar usuários.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 14,
                    Nome = "VISUALIZAR_AUDITORIA",
                    Descricao = "Permite visualizar auditorias e monitoramento.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 15,
                    Nome = "VISUALIZAR_DASHBOARD",
                    Descricao = "Permite visualizar o dashboard.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 16,
                    Nome = "VISUALIZAR_FILA",
                    Descricao = "Permite visualizar a fila de atendimento.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 17,
                    Nome = "VISUALIZAR_HISTORICO",
                    Descricao = "Permite visualizar o histórico de chamados.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 18,
                    Nome = "VISUALIZAR_NOTIFICACAO",
                    Descricao = "Permite visualizar notificações.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 19,
                    Nome = "VISUALIZAR_SLA",
                    Descricao = "Permite visualizar os SLAs.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 20,
                    Nome = "VISUALIZAR_TODOS_CHAMADOS",
                    Descricao = "Permite visualizar todos os chamados.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 21,
                    Nome = "VISUALIZAR_CATEGORIAS",
                    Descricao = "Permite gerenciar categorias de atendimento.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 22,
                    Nome = "DASHBOARD_ADMIN",
                    Descricao = "Permite visualizar o dashboard de administrador.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 23,
                    Nome = "DASHBOARD_TECNICO",
                    Descricao = "Permite visualizar o dashboard de técnico.",
                    Ativo = true
                },
                new
                {
                    Id_Permissao = 24,
                    Nome = "DASHBOARD_SOLICITANTE",
                    Descricao = "Permite visualizar o dashboard de solicitante.",
                    Ativo = true
                },
                                new
                {
                    Id_Permissao = 25,
                    Nome = "VISUALIZAR_AUDITORIA_CHAMADO",
                    Descricao = "Permite visualizar a auditoria de chamados.",
                    Ativo = true
                },
                
                new
                {
                    Id_Permissao = 26,
                    Nome = "VISUALIZAR_CENTRAL_ATENDIMENTO",
                    Descricao = "Permite visualizar a Central de Atendimento.",
                    Ativo = true
                }
            );
        }
    }
}
