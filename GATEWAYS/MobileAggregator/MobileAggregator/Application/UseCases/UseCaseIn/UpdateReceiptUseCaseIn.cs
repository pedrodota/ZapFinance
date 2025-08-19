namespace MobileAggregator.Application.UseCases.UseCaseIn;

public class UpdateReceiptUseCaseIn
{
    public string ReceiptId { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public decimal? Valor { get; set; }
    public string? Categoria { get; set; }
}
