using Grpc.Net.Client;
using MobileAggregator.Application.UseCases.IUseCase;
using MobileAggregator.Application.UseCases.UseCaseIn;
using MobileAggregator.Application.UseCases.UseCaseOut;
using ZapFinance.ProtoServer.Core;

namespace MobileAggregator.Application.UseCases.UseCase;

public class UserUseCase : IUserUseCase
{
    private readonly GrpcChannel _grpcChannel;
    private readonly ILogger<UserUseCase> _logger;

    public UserUseCase(GrpcChannel grpcChannel, ILogger<UserUseCase> logger)
    {
        _grpcChannel = grpcChannel;
        _logger = logger;
    }

    public async Task<CreateUserUseCaseOut> CreateUserAsync(CreateUserUseCaseIn request)
    {
        try
        {
            var client = new UsuarioService.UsuarioServiceClient(_grpcChannel);
            
            var grpcRequest = new CriarUsuarioRequest
            {
                Nome = request.Name,
                Email = request.Email,
                Telefone = request.Phone ?? string.Empty,
                Documento = request.Document,
                TipoDocumento = (TipoDocumento)request.DocumentType
            };

            var response = await client.CriarUsuarioAsync(grpcRequest);

            return new CreateUserUseCaseOut
            {
                Success = true,
                Message = "Usuário criado com sucesso",
                UserId = response.UsuarioId,
                User = MapToUserData(response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário {Email}", request.Email);
            return new CreateUserUseCaseOut
            {
                Success = false,
                Message = "Erro ao criar usuário"
            };
        }
    }

    public async Task<GetUserUseCaseOut> GetUserAsync(GetUserUseCaseIn request)
    {
        try
        {
            var client = new UsuarioService.UsuarioServiceClient(_grpcChannel);
            
            var grpcRequest = new ObterUsuarioRequest
            {
                UsuarioId = request.UserId
            };

            var response = await client.ObterUsuarioAsync(grpcRequest);

            return new GetUserUseCaseOut
            {
                Success = true,
                Message = "Usuário encontrado",
                User = MapToUserData(response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário {UserId}", request.UserId);
            return new GetUserUseCaseOut
            {
                Success = false,
                Message = "Usuário não encontrado"
            };
        }
    }

    public async Task<UpdateUserUseCaseOut> UpdateUserAsync(UpdateUserUseCaseIn request)
    {
        try
        {
            var client = new UsuarioService.UsuarioServiceClient(_grpcChannel);
            
            var grpcRequest = new AtualizarUsuarioRequest
            {
                UsuarioId = request.UserId,
                Nome = request.Name,
                Email = request.Email,
                Telefone = request.Phone ?? string.Empty,
                Ativo = request.Active
            };

            var response = await client.AtualizarUsuarioAsync(grpcRequest);

            return new UpdateUserUseCaseOut
            {
                Success = true,
                Message = "Usuário atualizado com sucesso",
                User = MapToUserData(response)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", request.UserId);
            return new UpdateUserUseCaseOut
            {
                Success = false,
                Message = "Erro ao atualizar usuário"
            };
        }
    }

    public async Task<DeleteUserUseCaseOut> DeleteUserAsync(DeleteUserUseCaseIn request)
    {
        try
        {
            var client = new UsuarioService.UsuarioServiceClient(_grpcChannel);
            
            var grpcRequest = new DeletarUsuarioRequest
            {
                UsuarioId = request.UserId
            };

            var response = await client.DeletarUsuarioAsync(grpcRequest);

            return new DeleteUserUseCaseOut
            {
                Success = response.Sucesso,
                Message = response.Mensagem
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar usuário {UserId}", request.UserId);
            return new DeleteUserUseCaseOut
            {
                Success = false,
                Message = "Erro ao deletar usuário"
            };
        }
    }

    public async Task<ListUsersUseCaseOut> ListUsersAsync(ListUsersUseCaseIn request)
    {
        try
        {
            var client = new UsuarioService.UsuarioServiceClient(_grpcChannel);
            
            var grpcRequest = new ListarUsuariosRequest
            {
                Pagina = request.Page,
                TamanhoPagina = request.PageSize,
                Filtro = request.Filter ?? string.Empty
            };

            var response = await client.ListarUsuariosAsync(grpcRequest);

            return new ListUsersUseCaseOut
            {
                Success = true,
                Message = "Usuários listados com sucesso",
                Users = response.Usuarios.Select(MapToUserData).ToList(),
                Total = response.Total,
                Page = response.Pagina,
                TotalPages = response.TotalPaginas
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return new ListUsersUseCaseOut
            {
                Success = false,
                Message = "Erro ao listar usuários"
            };
        }
    }

    private static UserData MapToUserData(UsuarioResponse response)
    {
        return new UserData
        {
            UserId = response.UsuarioId,
            Name = response.Nome,
            Email = response.Email,
            Phone = response.Telefone,
            Document = response.Documento,
            DocumentType = (int)response.TipoDocumento,
            Active = response.Ativo,
            CreatedAt = response.DataCriacao,
            UpdatedAt = response.DataAtualizacao
        };
    }
}
