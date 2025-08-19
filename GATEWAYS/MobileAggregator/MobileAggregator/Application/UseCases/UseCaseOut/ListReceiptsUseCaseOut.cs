namespace MobileAggregator.Application.UseCases.UseCaseOut;

public class ListReceiptsUseCaseOut
{
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Pagina { get; set; }
    public int TotalPaginas { get; set; }
}
