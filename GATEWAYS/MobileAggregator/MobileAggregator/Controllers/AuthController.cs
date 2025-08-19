using Microsoft.AspNetCore.Mvc;
using MobileAggregator.Application.UseCases.IUseCase;
using MobileAggregator.Application.UseCases.UseCaseIn;
using MobileAggregator.Application.UseCases.UseCaseOut;

namespace MobileAggregator.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthUseCase _authUseCase;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthUseCase authUseCase, ILogger<AuthController> logger)
    {
        _authUseCase = authUseCase;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginUseCaseOut>> Login([FromBody] LoginUseCaseIn request)
    {
        try
        {
            var result = await _authUseCase.LoginAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login para {Email}", request.Email);
            return StatusCode(500, new LoginUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<RefreshTokenUseCaseOut>> RefreshToken([FromBody] RefreshTokenUseCaseIn request)
    {
        try
        {
            var result = await _authUseCase.RefreshTokenAsync(request);
            
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return StatusCode(500, new RefreshTokenUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ChangePasswordUseCaseOut>> ChangePassword([FromBody] ChangePasswordUseCaseIn request)
    {
        try
        {
            var result = await _authUseCase.ChangePasswordAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha para usuário {UserId}", request.UserId);
            return StatusCode(500, new ChangePasswordUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ForgotPasswordUseCaseOut>> ForgotPassword([FromBody] ForgotPasswordUseCaseIn request)
    {
        try
        {
            var result = await _authUseCase.ForgotPasswordAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar recuperação de senha para {Email}", request.Email);
            return StatusCode(500, new ForgotPasswordUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }
}
