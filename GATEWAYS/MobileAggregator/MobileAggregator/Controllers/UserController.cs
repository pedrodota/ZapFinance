using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAggregator.Application.UseCases.IUseCase;
using MobileAggregator.Application.UseCases.UseCaseIn;
using MobileAggregator.Application.UseCases.UseCaseOut;

namespace MobileAggregator.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserUseCase _userUseCase;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserUseCase userUseCase, ILogger<UserController> logger)
    {
        _userUseCase = userUseCase;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<CreateUserUseCaseOut>> CreateUser([FromBody] CreateUserUseCaseIn request)
    {
        try
        {
            var result = await _userUseCase.CreateUserAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return CreatedAtAction(nameof(GetUser), new { id = result.UserId }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar usuário {Email}", request.Email);
            return StatusCode(500, new CreateUserUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetUserUseCaseOut>> GetUser(string id)
    {
        try
        {
            var request = new GetUserUseCaseIn { UserId = id };
            var result = await _userUseCase.GetUserAsync(request);
            
            if (!result.Success)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter usuário {UserId}", id);
            return StatusCode(500, new GetUserUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateUserUseCaseOut>> UpdateUser(string id, [FromBody] UpdateUserUseCaseIn request)
    {
        try
        {
            request.UserId = id;
            var result = await _userUseCase.UpdateUserAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário {UserId}", id);
            return StatusCode(500, new UpdateUserUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<DeleteUserUseCaseOut>> DeleteUser(string id)
    {
        try
        {
            var request = new DeleteUserUseCaseIn { UserId = id };
            var result = await _userUseCase.DeleteUserAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar usuário {UserId}", id);
            return StatusCode(500, new DeleteUserUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }

    [HttpGet]
    public async Task<ActionResult<ListUsersUseCaseOut>> ListUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? filter = null)
    {
        try
        {
            var request = new ListUsersUseCaseIn 
            { 
                Page = page, 
                PageSize = pageSize, 
                Filter = filter 
            };
            
            var result = await _userUseCase.ListUsersAsync(request);
            
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return StatusCode(500, new ListUsersUseCaseOut
            {
                Success = false,
                Message = "Erro interno do servidor"
            });
        }
    }
}
