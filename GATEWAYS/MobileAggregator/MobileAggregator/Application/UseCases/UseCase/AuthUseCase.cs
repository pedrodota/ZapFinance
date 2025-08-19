using Grpc.Net.Client;
using MobileAggregator.Application.UseCases.IUseCase;
using MobileAggregator.Application.UseCases.UseCaseIn;
using MobileAggregator.Application.UseCases.UseCaseOut;
using ZapFinance.ProtoServer.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace MobileAggregator.Application.UseCases.UseCase;

public class AuthUseCase : IAuthUseCase
{
    private readonly GrpcChannel _grpcChannel;
    private readonly ILogger<AuthUseCase> _logger;
    private readonly IConfiguration _configuration;

    public AuthUseCase(GrpcChannel grpcChannel, ILogger<AuthUseCase> logger, IConfiguration configuration)
    {
        _grpcChannel = grpcChannel;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<LoginUseCaseOut> LoginAsync(LoginUseCaseIn request)
    {
        try
        {
            // Usar Core Service em vez de IdentityService
            var client = new ZapFinance.ProtoServer.Core.UsuarioService.UsuarioServiceClient(_grpcChannel);
            
            var grpcRequest = new ZapFinance.ProtoServer.Core.ListarUsuariosRequest
            {
                Email = request.Email,
                Pagina = 1,
                TamanhoPagina = 1
            };

            var response = await client.ListarUsuariosAsync(grpcRequest);
            
            if (response.Usuarios.Count == 0)
            {
                return new LoginUseCaseOut
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                };
            }

            var usuario = response.Usuarios.First();
            
            // Verificar senha
            if (usuario.Senha != request.Password)
            {
                return new LoginUseCaseOut
                {
                    Success = false,
                    Message = "Email ou senha inválidos"
                };
            }

            if (!usuario.Ativo)
            {
                return new LoginUseCaseOut
                {
                    Success = false,
                    Message = "Usuário inativo"
                };
            }

            // Gerar token JWT (implementação simplificada)
            var token = GenerateJwtToken(usuario);

            return new LoginUseCaseOut
            {
                Success = true,
                Message = "Login realizado com sucesso",
                Token = token,
                RefreshToken = Guid.NewGuid().ToString(),
                ExpiresIn = 3600,
                User = new UserInfo
                {
                    UserId = usuario.UsuarioId,
                    Email = usuario.Email,
                    Name = usuario.Nome,
                    Roles = new List<string> { "User" },
                    Permissions = new List<string> { "user:read", "user:write" },
                    Active = usuario.Ativo
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao autenticar usuário {Email}", request.Email);
            return new LoginUseCaseOut
            {
                Success = false,
                Message = "Erro ao processar autenticação"
            };
        }
    }

    public async Task<RefreshTokenUseCaseOut> RefreshTokenAsync(RefreshTokenUseCaseIn request)
    {
        try
        {
            var client = new IdentityService.IdentityServiceClient(_grpcChannel);
            
            var grpcRequest = new RefreshTokenRequest
            {
                RefreshToken = request.RefreshToken
            };

            var response = await client.RefreshTokenAsync(grpcRequest);

            return new RefreshTokenUseCaseOut
            {
                Success = response.Sucesso,
                Message = response.Mensagem,
                Token = response.Token,
                RefreshToken = response.RefreshToken,
                ExpiresIn = response.ExpiresIn
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return new RefreshTokenUseCaseOut
            {
                Success = false,
                Message = "Erro ao processar renovação do token"
            };
        }
    }

    public async Task<ChangePasswordUseCaseOut> ChangePasswordAsync(ChangePasswordUseCaseIn request)
    {
        try
        {
            var client = new IdentityService.IdentityServiceClient(_grpcChannel);
            
            var grpcRequest = new AlterarSenhaRequest
            {
                UsuarioId = request.UserId,
                SenhaAtual = request.CurrentPassword,
                NovaSenha = request.NewPassword
            };

            var response = await client.AlterarSenhaAsync(grpcRequest);

            return new ChangePasswordUseCaseOut
            {
                Success = response.Sucesso,
                Message = response.Mensagem
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha do usuário {UserId}", request.UserId);
            return new ChangePasswordUseCaseOut
            {
                Success = false,
                Message = "Erro ao processar alteração de senha"
            };
        }
    }

    public async Task<ForgotPasswordUseCaseOut> ForgotPasswordAsync(ForgotPasswordUseCaseIn request)
    {
        try
        {
            var client = new IdentityService.IdentityServiceClient(_grpcChannel);
            
            var grpcRequest = new RecuperarSenhaRequest
            {
                Email = request.Email
            };

            var response = await client.RecuperarSenhaAsync(grpcRequest);

            return new ForgotPasswordUseCaseOut
            {
                Success = response.Sucesso,
                Message = response.Mensagem
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar recuperação de senha para {Email}", request.Email);
            return new ForgotPasswordUseCaseOut
            {
                Success = false,
                Message = "Erro ao processar recuperação de senha"
            };
        }
    }

    private string GenerateJwtToken(ZapFinance.ProtoServer.Core.UsuarioResponse usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId),
            new Claim(ClaimTypes.Role, "User"),
            new Claim("permissions", "user:read,user:write")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
