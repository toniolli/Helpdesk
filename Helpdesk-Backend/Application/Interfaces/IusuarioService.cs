using Application.Dto.Autenticacao;
using Application.Dto.UsuarioDto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IusuarioService
    {
        Task<CreateUserDto> CriarUsuario(CreateUserDto createDTO);
        Task<UpdateUserDto> Atualizar(string id, UpdateUserDto updateUserDto);
        Task<ResultadoAutenticacaoDto> Login(LoginDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<UsuarioReponseDto>> BuscarTodos();
        Task<UsuarioReponseDto> BuscarPorIdInterno (string id) ;
        Task<UsuarioReponseDto> BuscarPorId(string id,string usuarioLogadoId,bool podeGerenciarUsuarios);
        Task<CreateUserDto?> Excluir(string id);
        Task<string> ChangePassword(string userIdLogado, ChangePasswordDto dto, bool isAdmin);
        Task<string> RestaurarSenhaPadrao(string usuarioID);
        Task RecuperarSenha(RecuperarSenhaDto recuperarDto);
        Task Ativar(string id);
        Task Desativar(string id);
        Task<UsuarioLogadoDto> ObterUsuarioLogado();
        Task<List<UsuarioResumoDto>> BuscarUsuariosTecnicos();
        Task<List<UsuarioResumoDto>> BuscarUsuariosDistribuicao();
        Task<List<UsuarioResumoDto>> BuscarUsuariosHistorico();
    }

}





