using Core.Service.Application.Domain.Models;

namespace Core.Service.Infrastructure.Data.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id);
    Task<Usuario?> ObterPorEmailAsync(string email);
    Task<Usuario?> ObterPorDocumentoAsync(string documento);
    Task<Usuario?> ObterPorWhatsAppAsync(string whatsAppNumber);
    Task<IEnumerable<Usuario>> ListarAsync(int pagina, int tamanhoPagina, string? filtro = null);
    Task<int> ContarAsync(string? filtro = null);
    Task<Usuario> CriarAsync(Usuario usuario);
    Task<Usuario> AtualizarAsync(Usuario usuario);
    Task<bool> DeletarAsync(Guid id);
    Task<bool> ExisteEmailAsync(string email);
    Task<bool> ExisteDocumentoAsync(string documento);
}
