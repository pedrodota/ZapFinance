using Core.Service.Application.Domain.Models;
using Core.Service.Infrastructure.UnityOfWork;
using Grpc.Core;
using ZapFinance.ProtoServer.Core;
using Models = Core.Service.Application.Domain.Models;

namespace Core.Service.Infrastructure.Adapter;

public class UsuarioGrpcService : UsuarioService.UsuarioServiceBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UsuarioGrpcService> _logger;

    public UsuarioGrpcService(IUnitOfWork unitOfWork, ILogger<UsuarioGrpcService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public override async Task<UsuarioResponse> CriarUsuario(CriarUsuarioRequest request, ServerCallContext context)
    {
        try
        {
            // Validar se email já existe
            if (await _unitOfWork.Usuarios.ExisteEmailAsync(request.Email))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Email já cadastrado"));
            }

            // Validar se documento já existe
            if (await _unitOfWork.Usuarios.ExisteDocumentoAsync(request.Documento))
            {
                throw new RpcException(new Status(StatusCode.AlreadyExists, "Documento já cadastrado"));
            }

            var usuario = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = request.Nome,
                Email = request.Email,
                Telefone = request.Telefone,
                Documento = request.Documento,
                TipoDocumento = (Models.TipoDocumento)request.TipoDocumento,
                DataCriacao = DateTime.UtcNow
            };

            await _unitOfWork.Usuarios.CriarAsync(usuario);

            return MapToResponse(usuario);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário");
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    public override async Task<UsuarioResponse> ObterUsuario(ObterUsuarioRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UsuarioId, out var usuarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID de usuário inválido"));
            }

            var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Usuário não encontrado"));
            }

            return MapToResponse(usuario);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário {UsuarioId}", request.UsuarioId);
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    public override async Task<UsuarioResponse> AtualizarUsuario(AtualizarUsuarioRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UsuarioId, out var usuarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID de usuário inválido"));
            }

            var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(usuarioId);
            if (usuario == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Usuário não encontrado"));
            }

            usuario.Atualizar(request.Nome, request.Email, request.Telefone);
            
            if (request.Ativo)
                usuario.Ativar();
            else
                usuario.Desativar();

            await _unitOfWork.Usuarios.AtualizarAsync(usuario);

            return MapToResponse(usuario);
        }
        catch (RpcException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {UsuarioId}", request.UsuarioId);
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    public override async Task<ListarUsuariosResponse> ListarUsuarios(ListarUsuariosRequest request, ServerCallContext context)
    {
        try
        {
            _logger.LogInformation("Iniciando ListarUsuarios - Email: {Email}, Pagina: {Pagina}, TamanhoPagina: {TamanhoPagina}", 
                request.Email, request.Pagina, request.TamanhoPagina);

            // Se email específico for fornecido, usar como filtro
            var filtro = !string.IsNullOrEmpty(request.Email) ? request.Email : request.Filtro;
            
            _logger.LogInformation("Consultando usuários com filtro: {Filtro}", filtro);
            
            var usuarios = await _unitOfWork.Usuarios.ListarAsync(
                request.Pagina, 
                request.TamanhoPagina, 
                filtro);

            _logger.LogInformation("Encontrados {Count} usuários", usuarios.Count());

            var total = await _unitOfWork.Usuarios.ContarAsync(filtro);
            var totalPaginas = (int)Math.Ceiling((double)total / request.TamanhoPagina);

            var response = new ListarUsuariosResponse
            {
                Total = total,
                Pagina = request.Pagina,
                TotalPaginas = totalPaginas
            };

            response.Usuarios.AddRange(usuarios.Select(MapToResponse));

            _logger.LogInformation("ListarUsuarios concluído com sucesso - Total: {Total}, Páginas: {TotalPaginas}", total, totalPaginas);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários - Tipo: {ExceptionType}, Mensagem: {Message}", 
                ex.GetType().Name, ex.Message);
            
            // Provide more specific error information in development
            var errorMessage = "Erro interno do servidor";
            if (ex.InnerException != null)
            {
                _logger.LogError("Inner Exception: {InnerExceptionType} - {InnerMessage}", 
                    ex.InnerException.GetType().Name, ex.InnerException.Message);
            }
            
            throw new RpcException(new Status(StatusCode.Internal, errorMessage));
        }
    }

    public override async Task<DeletarUsuarioResponse> DeletarUsuario(DeletarUsuarioRequest request, ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.UsuarioId, out var usuarioId))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "ID de usuário inválido"));
            }

            var sucesso = await _unitOfWork.Usuarios.DeletarAsync(usuarioId);

            return new DeletarUsuarioResponse
            {
                Sucesso = sucesso,
                Mensagem = sucesso ? "Usuário deletado com sucesso" : "Usuário não encontrado"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar usuário {UsuarioId}", request.UsuarioId);
            throw new RpcException(new Status(StatusCode.Internal, "Erro interno do servidor"));
        }
    }

    private static UsuarioResponse MapToResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            UsuarioId = usuario.Id.ToString(),
            Nome = usuario.Nome,
            Email = usuario.Email,
            Telefone = usuario.Telefone ?? string.Empty,
            Documento = usuario.Documento,
            TipoDocumento = (ZapFinance.ProtoServer.Core.TipoDocumento)((int)usuario.TipoDocumento),
            Ativo = usuario.Ativo,
            DataCriacao = usuario.DataCriacao.ToString("yyyy-MM-ddTHH:mm:ssZ"),
            DataAtualizacao = usuario.DataAtualizacao?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty,
            Senha = usuario.Senha ?? string.Empty
        };
    }
}
