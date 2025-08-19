using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MobileAggregator.Application.UseCases.UploadReceiptUseCase;

namespace MobileAggregator.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class ReceiptController : ControllerBase
{
    private readonly IUploadReceiptUseCase _uploadReceiptUseCase;
    private readonly ILogger<ReceiptController> _logger;

    public ReceiptController(IUploadReceiptUseCase uploadReceiptUseCase, ILogger<ReceiptController> logger)
    {
        _uploadReceiptUseCase = uploadReceiptUseCase;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(15 * 1024 * 1024)] // 15MB to allow some headroom over 10MB app limit
    [AllowAnonymous]
    public async Task<ActionResult<UploadReceiptUseCaseOut>> Upload([FromForm] UploadReceiptUseCaseIn request)
    {
        try
        {
            var result = await _uploadReceiptUseCase.ExecuteAsync(request);

            if (!result.Sucesso)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload de recibo para o usuário {UsuarioId}", request.UsuarioId);
            return StatusCode(500, new UploadReceiptUseCaseOut
            {
                Sucesso = false,
                Mensagem = "Erro interno do servidor"
            });
        }
    }
}
