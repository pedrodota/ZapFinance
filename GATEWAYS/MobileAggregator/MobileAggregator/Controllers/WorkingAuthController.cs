using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Grpc.Net.Client;
using ZapFinance.ProtoServer.Core;

namespace MobileAggregator.Controllers;

[ApiController]
[Route("api/working-auth")]
public class WorkingAuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<WorkingAuthController> _logger;
    private readonly GrpcChannel _grpcChannel;

    public WorkingAuthController(IConfiguration configuration, ILogger<WorkingAuthController> logger, GrpcChannel grpcChannel)
    {
        _configuration = configuration;
        _logger = logger;
        _grpcChannel = grpcChannel;
    }

    [HttpPost("login")]
    public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Tentativa de login para: {Email}", request.Email);

            // Buscar usuário no Core Service via gRPC
            var client = new UsuarioService.UsuarioServiceClient(_grpcChannel);
            
            var grpcRequest = new ListarUsuariosRequest
            {
                Email = request.Email
            };

            var response = await client.ListarUsuariosAsync(grpcRequest);
            
            if (response.Usuarios.Count == 0)
            {
                _logger.LogWarning("Usuário não encontrado: {Email}", request.Email);
                return Unauthorized(new { success = false, message = "Email ou senha inválidos" });
            }

            var usuario = response.Usuarios.First();
            
            // Verificar senha (comparação simples para teste)
            if (usuario.Senha != request.Password)
            {
                _logger.LogWarning("Senha incorreta para: {Email}", request.Email);
                return Unauthorized(new { success = false, message = "Email ou senha inválidos" });
            }

            if (!usuario.Ativo)
            {
                _logger.LogWarning("Usuário inativo: {Email}", request.Email);
                return Unauthorized(new { success = false, message = "Usuário inativo" });
            }

            // Gerar JWT Token
            var token = GenerateJwtToken(usuario);

            _logger.LogInformation("Login bem-sucedido para: {Email}", request.Email);

            return Ok(new
            {
                success = true,
                message = "Login realizado com sucesso",
                token = token,
                refreshToken = Guid.NewGuid().ToString(),
                expiresIn = 3600,
                user = new
                {
                    userId = usuario.UsuarioId,
                    email = usuario.Email,
                    name = usuario.Nome,
                    phone = usuario.Telefone,
                    document = usuario.Documento,
                    active = usuario.Ativo
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no login para {Email}", request.Email);
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    private string GenerateJwtToken(UsuarioResponse usuario)
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

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
