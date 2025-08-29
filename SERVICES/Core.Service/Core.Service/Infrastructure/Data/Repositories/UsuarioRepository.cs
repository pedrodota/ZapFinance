using Core.Service.Application.Domain.Models;
using Core.Service.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Core.Service.Infrastructure.Data.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ZapFinanceDbContext _context;

    public UsuarioRepository(ZapFinanceDbContext context)
    {
        _context = context; 
    }

    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == id && u.Ativo);
    }

    public async Task<Usuario?> ObterPorEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email && u.Ativo);
    }

    public async Task<Usuario?> ObterPorDocumentoAsync(string documento)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Documento == documento && u.Ativo);
    }

    public async Task<Usuario?> ObterPorWhatsAppAsync(string whatsAppNumber)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.WhatsAppNumber == whatsAppNumber && u.Ativo);
    }

    public async Task<IEnumerable<Usuario>> ListarAsync(int pagina, int tamanhoPagina, string? filtro = null)
    {
        var query = _context.Usuarios.Where(u => u.Ativo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(u => 
                u.Nome.Contains(filtro) || 
                u.Email.Contains(filtro) || 
                u.Documento.Contains(filtro));
        }

        return await query
            .OrderBy(u => u.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarAsync(string? filtro = null)
    {
        var query = _context.Usuarios.Where(u => u.Ativo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(u => 
                u.Nome.Contains(filtro) || 
                u.Email.Contains(filtro) || 
                u.Documento.Contains(filtro));
        }

        return await query.CountAsync();
    }

    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> DeletarAsync(Guid id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null) return false;

        usuario.Desativar();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExisteEmailAsync(string email)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Email == email && u.Ativo);
    }

    public async Task<bool> ExisteDocumentoAsync(string documento)
    {
        return await _context.Usuarios
            .AnyAsync(u => u.Documento == documento && u.Ativo);
    }
}
