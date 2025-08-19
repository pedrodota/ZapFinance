using Microsoft.AspNetCore.Http;

namespace MobileAggregator.Application.UseCases.UploadReceiptUseCase;

public class UploadReceiptUseCaseIn
{
    public string UsuarioId { get; set; } = string.Empty;
    public IFormFile Arquivo { get; set; } = null!;
    public string? Descricao { get; set; }
    public decimal? Valor { get; set; }
    public string? Categoria { get; set; }
}
