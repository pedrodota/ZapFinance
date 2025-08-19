namespace MobileAggregator.Application.UseCases.UploadReceiptUseCase;

public class UploadReceiptUseCaseOut
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public ReceiptDto? Receipt { get; set; }
}

public class ReceiptDto
{
    public string Id { get; set; } = string.Empty;
    public string UsuarioId { get; set; } = string.Empty;
    public string NomeArquivo { get; set; } = string.Empty;
    public string CaminhoArquivo { get; set; } = string.Empty;
    public string TipoMime { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal? Valor { get; set; }
    public string? Categoria { get; set; }
    public DateTime DataUpload { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    public bool Ativo { get; set; }
}
