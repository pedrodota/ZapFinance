using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MobileAggregator.Controllers;

[ApiController]
[Route("api/test-auth")]
public class TestAuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TestAuthController> _logger;

    public TestAuthController(IConfiguration configuration, ILogger<TestAuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public ActionResult<object> TestLogin([FromBody] TestLoginRequest request)
    {
        try
        {
            // Para teste, aceita qualquer email/senha
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { success = false, message = "Email e senha são obrigatórios" });
            }

            // Gerar JWT Token
            var token = GenerateJwtToken(request.Email);

            return Ok(new
            {
                success = true,
                message = "Login realizado com sucesso",
                token = token,
                refreshToken = Guid.NewGuid().ToString(),
                expiresIn = 3600,
                user = new
                {
                    userId = Guid.NewGuid().ToString(),
                    email = request.Email,
                    name = "Usuário Teste",
                    roles = new[] { "User" },
                    permissions = new[] { "user:read", "user:write" },
                    active = true
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no login de teste");
            return StatusCode(500, new { success = false, message = "Erro interno do servidor" });
        }
    }

    private string GenerateJwtToken(string email)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, "Usuário Teste"),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
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

public class TestLoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
