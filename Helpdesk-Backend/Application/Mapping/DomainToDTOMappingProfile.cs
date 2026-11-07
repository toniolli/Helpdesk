
using Application.Dto.Auditoria;
using Application.Dto.CategoriaAtendimentoDto;
using Application.Dto.Chamado;
using Application.Dto.Comentario;
using Application.Dto.EmailNotification;
using Application.Dto.EmailNotificationDto;
using Application.Dto.NotificacaoDto;
using Application.Dto.SlaDto;
using Application.Dto.UsuarioDto;
using Application.DTOs;
using Application.DTOs.Anexo;
using AutoMapper;
using Domain.Entities;
using Application.Dto.AuditoriaIdentity;
using Application.Dto.AuditoriaChamado;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Mapping
{
    public class DomainToDTOMappingProfile : Profile
    {
        public DomainToDTOMappingProfile()
        {
            CreateMap<PeriodoTrabalhoPerfil, PeriodoTrabalhoPerfilDto>();
            CreateMap<Perfil, PerfilDto>()
                .ForMember(
                    destino => destino.PossuiPeriodoTrabalho,
                    opcao => opcao.MapFrom(
                    origem => origem.PeriodoTrabalhoVigente != null))
                .ForMember(
                    destino => destino.PeriodoTrabalho,
                    opcao => opcao.MapFrom(
                    origem => origem.PeriodoTrabalhoVigente));
            CreateMap<Permissao,PermissaoDto>().ReverseMap();
            CreateMap<PerfilPermissao, PerfilPermissaoDto>().ReverseMap();
            CreateMap<UsuarioPerfil, UsuarioPerfilDto>().ReverseMap();
            CreateMap<CategoriaAtendimento, CategoriaAtendimentoResponseDto>().ReverseMap();
            CreateMap<CategoriaAtendimento, CategoriaAtendimentoRequestDto>().ReverseMap();
            CreateMap<ChamadoResponseDto, Chamado>();
            CreateMap<Chamado, ChamadoRequestDto>().ReverseMap();
            CreateMap<Comentario, ComentarioResponseDto>().ReverseMap();
            CreateMap<Comentario, ComentarioRequestDto>().ReverseMap();
            CreateMap<Anexo, AnexoResponseDto>().ReverseMap();
            CreateMap<Notificacao, NotificacaoRequestDto>().ReverseMap();
            CreateMap<Notificacao, NotificacaoResponseDto>().ReverseMap();
            CreateMap<EmailNotification, EmailNotificationRequestDto>().ReverseMap();
            CreateMap<EmailNotification, EmailNotificationResponseDto>().ReverseMap();
            CreateMap<SLA, SlaRequestDto>().ReverseMap();
            CreateMap<SLA, SlaResponseDto>().ReverseMap();
            CreateMap<RegraDistribuicao, RegraDistribuicaoRequestDto>().ReverseMap();
            CreateMap<RegraDistribuicao, RegraDistribuicaoResponseDto>().ReverseMap();
            CreateMap<CriterioDistribuicao, CriterioDistribuicaoRequestDto>().ReverseMap();
            CreateMap<CriterioDistribuicao, CriterioDistribuicaoResponseDto>().ReverseMap();
            CreateMap<Auditoria, AuditoriaResponseDto>();
            CreateMap<AuditoriaIdentity, AuditoriaIdentityResponseDto>();
            CreateMap<AuditoriaChamado, AuditoriaChamadoResponseDto>();

            CreateMap<Chamado, ChamadoResponseDto>().AfterMap((chamado, dto) =>
            {
                var agora = DateTime.Now;

                dto.TempoCorridoHoras =
                    chamado.CalcularTempoCorridoHoras(agora);

                dto.TempoPrimeiroAtendimentoHoras =
                    chamado.CalcularTempoPrimeiroAtendimentoHoras();

                dto.TempoPrimeiroAtendimentoSlaHoras =
                    chamado.CalcularTempoPrimeiroAtendimentoSlaHoras(agora);

                dto.TempoAguardandoRetornoHoras =
                    chamado.CalcularTempoAguardandoRetornoHoras(agora);

                dto.TempoAguardandoEquipamentoHoras =
                    chamado.CalcularTempoAguardandoEquipamentoHoras(agora);

                dto.TempoTotalPausadoHoras =
                    chamado.CalcularTempoTotalPausadoHoras(agora);

                dto.TempoResolucaoSlaHoras =
                    chamado.CalcularTempoResolucaoSlaHoras(agora);

                dto.SlaPendenteDefinicaoPerfil =
                    !dto.TempoPrimeiroAtendimentoSlaHoras.HasValue ||
                    !dto.TempoResolucaoSlaHoras.HasValue;

                dto.TempoEfetivoAtendimentoHoras =
                    chamado.CalcularTempoEfetivoAtendimentoHoras(agora);
            });
        }
    }
}

