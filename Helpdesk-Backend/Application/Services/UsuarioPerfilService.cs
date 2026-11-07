using Application.Dto.UsuarioDto;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena consultas e alterações dos vínculos entre usuários e perfis.
    public class UsuarioPerfilService : IUsuarioPerfilService
    {
        private readonly IMapper _mapper;
        private readonly IusuarioPerfilRepository _usuarioPerfilRepository;

        public UsuarioPerfilService(
            IMapper mapper,
            IusuarioPerfilRepository usuarioPerfilRepository)
        {
            _mapper = mapper;
            _usuarioPerfilRepository = usuarioPerfilRepository;
        }

        // CONSULTAS

        public async Task<IEnumerable<PerfilDto>> BuscarPerfisUsuario(
            string usuarioId)
        {
            var perfis =
                await _usuarioPerfilRepository.BuscarPerfisUsuario(
                    usuarioId);

            return _mapper.Map<IEnumerable<PerfilDto>>(perfis);
        }

        public async Task<IEnumerable<string>> BuscarUsuariosComPermissao(
            string nomePermissao)
        {
            return await _usuarioPerfilRepository
                .BuscarUsuarioComPermissao(nomePermissao);
        }

        public async Task<IEnumerable<UsuarioPerfilDto>> BuscarUsuariosPerfil(
            int perfilId)
        {
            var usuarios =
                await _usuarioPerfilRepository.BuscarUsuariosPerfil(
                    perfilId);

            return _mapper.Map<IEnumerable<UsuarioPerfilDto>>(usuarios);
        }

        // ALTERAÇÃO DE VÍNCULOS

        // Mantém o retorno nulo quando não existe vínculo para remover.
        public async Task<UsuarioPerfilDto?> Desvincular(
            string usuarioId,
            int perfilId)
        {
            var resultado =
                await _usuarioPerfilRepository.Desvincular(
                    usuarioId,
                    perfilId);

            if (resultado == null)
                return null;

            return _mapper.Map<UsuarioPerfilDto>(resultado);
        }

        // Impede que o mesmo perfil seja vinculado novamente ao usuário.
        public async Task<UsuarioPerfilDto> Vincular(
            UsuarioPerfilDto usuarioPerfilDto)
        {
            var existeVinculo = await _usuarioPerfilRepository
                .ExisteVinculo(
                    usuarioPerfilDto.UsuarioId,
                    usuarioPerfilDto.PerfilId);

            if (existeVinculo)
                throw new DomainExceptionValidation(
                    "Perfil já vinculado ao usuário");

            var entidade =
                _mapper.Map<UsuarioPerfil>(usuarioPerfilDto);

            var resultado =
                await _usuarioPerfilRepository.Vincular(entidade);

            return _mapper.Map<UsuarioPerfilDto>(resultado);
        }
    }
}
