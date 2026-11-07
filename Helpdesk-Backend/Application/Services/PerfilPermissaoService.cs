using Application.Dto.UsuarioDto;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena consultas e alterações dos vínculos entre perfis e permissões.
    public class PerfilPermissaoService : IperfilPermissaoService
    {
        private readonly IMapper _mapper;
        private readonly IperfilPermissaoRepository _perfilPermissaoRepository;
        private readonly IUsuarioPerfilService _usuarioPerfilService;

        public PerfilPermissaoService(
            IMapper mapper,
            IperfilPermissaoRepository perfilPermissaoRepository,
            IUsuarioPerfilService usuarioPerfilService)
        {
            _mapper = mapper;
            _perfilPermissaoRepository = perfilPermissaoRepository;
            _usuarioPerfilService = usuarioPerfilService;
        }

        // CONSULTAS

        public async Task<IEnumerable<PerfilDto>> BuscarPerfisPermissao(
            int permissaoId)
        {
            var perfilPermissao =
                await _perfilPermissaoRepository.BuscarPerfisPermissao(
                    permissaoId);

            return _mapper.Map<IEnumerable<PerfilDto>>(perfilPermissao);
        }

        public async Task<IEnumerable<PermissaoDto>> BuscarPermissoesPerfil(
            int perfilId)
        {
            var perfilPermissao =
                await _perfilPermissaoRepository.BuscarPermissoesPerfil(
                    perfilId);

            return _mapper.Map<IEnumerable<PermissaoDto>>(perfilPermissao);
        }

        // ALTERAÇÃO DE VÍNCULOS

        // Mantém o retorno nulo quando não há vínculo para remover.
        public async Task<PerfilPermissaoDto?> Desvincular(
            int perfilId,
            int permissaoId)
        {
            var perfilPermissao =
                await _perfilPermissaoRepository.Desvincular(
                    perfilId,
                    permissaoId);

            if (perfilPermissao == null)
                return null;

            return _mapper.Map<PerfilPermissaoDto>(perfilPermissao);
        }

        // Evita que a mesma permissão seja vinculada novamente ao perfil.
        public async Task<PerfilPermissaoDto> Vincular(
            PerfilPermissaoDto perfilPermissaoDto)
        {
            var existeVinculo =
                await _perfilPermissaoRepository.ExisteVinculo(
                    perfilPermissaoDto.PerfilId,
                    perfilPermissaoDto.PermissaoId);

            if (existeVinculo)
                throw new DomainExceptionValidation(
                    "Prmissao ja vinculada ao perfil");

            var entidade =
                _mapper.Map<PerfilPermissao>(perfilPermissaoDto);

            var resultado =
                await _perfilPermissaoRepository.Vincular(entidade);

            return _mapper.Map<PerfilPermissaoDto>(resultado);
        }
    }
}
