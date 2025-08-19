namespace MobileAggregator.Application.UseCases.UseCaseIn;

public class ListReceiptsUseCaseIn
{
    public string UsuarioId { get; set; } = string.Empty;
    public int Pagina { get; set; } = 1;
    public int TamanhoPagina { get; set; } = 10;
    public string? Filtro { get; set; }
    public string? Categoria { get; set; }
}
