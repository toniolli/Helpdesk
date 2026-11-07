using Application.Dto.UsuarioDto;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using Domain.Interfaces;
using Domain.Validation;

namespace Application.Services
{
    // Coordena o cadastro, a consulta e as alterações de estado das permissões.
    public class PermissaoService : IPermissaoService
    {
        private readonly IMapper _mapper;
        private readonly IpermissaoRepository _permissaoRepository;

        public PermissaoService(
            IMapper mapper,
            IpermissaoRepository permissaoRepository)
        {
            _mapper = mapper;
            _permissaoRepository = permissaoRepository;
        }

        // CRIAÇÃO

        public async Task<PermissaoDto> Adicionar(
            PermissaoDto permissaoDto)
        {
            var permissao =
                _mapper.Map<Permissao>(permissaoDto);

            var criar =
                await _permissaoRepository.Adicionar(permissao);

            return _mapper.Map<PermissaoDto>(criar);
        }

        // ALTERAÇÃO

        public async Task<PermissaoDto> Atualizar(
            int id,
            PermissaoDto permissaoDto)
        {
            var permissao =
                await _permissaoRepository.BuscarPorId(id);

            if (permissao == null)
                throw new DomainExceptionValidation(
                    "permissao não encontrada.");

            permissao.Atualizar(
                permissaoDto.Nome,
                permissaoDto.Descricao);

            return _mapper.Map<PermissaoDto>(permissao);
        }

        // CONSULTAS

        public async Task<PermissaoDto?> BuscarPorId(int id)
        {
            var permissao =
                await _permissaoRepository.BuscarPorId(id);

            return _mapper.Map<PermissaoDto>(permissao);
        }

        public async Task<IEnumerable<PermissaoDto>> BuscarTodos()
        {
            var permissao =
                await _permissaoRepository.BuscarTodos();

            return _mapper.Map<IEnumerable<PermissaoDto>>(permissao);
        }

        // REMOÇÃO E ESTADO

        public async Task Remover(int id)
        {
            await _permissaoRepository.Remover(id);
        }

        public async Task Ativar(int id)
        {
            var permissao =
                await _permissaoRepository.BuscarPorId(id);

            if (permissao == null)
                throw new DomainExceptionValidation(
                    "Permissao de atendimento não encontrada.");

            permissao.Ativar();

            await _permissaoRepository.Atualizar(permissao);
        }

        public async Task Desativar(int id)
        {
            var permissao =
                await _permissaoRepository.BuscarPorId(id);

            if (permissao == null)
                throw new DomainExceptionValidation(
                    "Permissao de atendimento não encontrada.");

            permissao.Desativar();

            await _permissaoRepository.Atualizar(permissao);
        }
    }
}
