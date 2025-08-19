using Core.Service.Application.Domain.Models;

namespace Core.Service.Infrastructure.Data.Repositories;

public interface IReceiptRepository
{
    Task<Receipt?> ObterPorIdAsync(Guid id);
    Task<IEnumerable<Receipt>> ListarPorUsuarioAsync(Guid usuarioId, int pagina, int tamanhoPagina, string? filtro = null, string? categoria = null);
    Task<int> ContarPorUsuarioAsync(Guid usuarioId, string? filtro = null, string? categoria = null);
    Task<Receipt> CriarAsync(Receipt receipt);
    Task<Receipt> AtualizarAsync(Receipt receipt);
    Task<bool> DeletarAsync(Guid id);
    Task<bool> ExisteAsync(Guid id);
    Task<IEnumerable<Receipt>> ListarPorCategoriaAsync(string categoria, Guid usuarioId);
}
