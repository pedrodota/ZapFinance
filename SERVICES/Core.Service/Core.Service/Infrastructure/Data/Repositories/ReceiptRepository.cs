using Core.Service.Application.Domain.Models;
using Core.Service.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Core.Service.Infrastructure.Data.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly ZapFinanceDbContext _context;

    public ReceiptRepository(ZapFinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Receipt?> ObterPorIdAsync(Guid id)
    {
        return await _context.Receipts
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.Id == id && r.Ativo);
    }

    public async Task<IEnumerable<Receipt>> ListarPorUsuarioAsync(Guid usuarioId, int pagina, int tamanhoPagina, string? filtro = null, string? categoria = null)
    {
        var query = _context.Receipts
            .Include(r => r.Usuario)
            .Where(r => r.UsuarioId == usuarioId && r.Ativo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(r => 
                r.NomeArquivo.Contains(filtro) || 
                (r.Descricao != null && r.Descricao.Contains(filtro)) ||
                (r.Categoria != null && r.Categoria.Contains(filtro)));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(r => r.Categoria == categoria);
        }

        return await query
            .OrderByDescending(r => r.DataUpload)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync();
    }

    public async Task<int> ContarPorUsuarioAsync(Guid usuarioId, string? filtro = null, string? categoria = null)
    {
        var query = _context.Receipts.Where(r => r.UsuarioId == usuarioId && r.Ativo);

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(r => 
                r.NomeArquivo.Contains(filtro) || 
                (r.Descricao != null && r.Descricao.Contains(filtro)) ||
                (r.Categoria != null && r.Categoria.Contains(filtro)));
        }

        if (!string.IsNullOrWhiteSpace(categoria))
        {
            query = query.Where(r => r.Categoria == categoria);
        }

        return await query.CountAsync();
    }

    public async Task<Receipt> CriarAsync(Receipt receipt)
    {
        _context.Receipts.Add(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }

    public async Task<Receipt> AtualizarAsync(Receipt receipt)
    {
        _context.Receipts.Update(receipt);
        await _context.SaveChangesAsync();
        return receipt;
    }

    public async Task<bool> DeletarAsync(Guid id)
    {
        var receipt = await _context.Receipts.FindAsync(id);
        if (receipt == null) return false;

        receipt.Desativar();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExisteAsync(Guid id)
    {
        return await _context.Receipts
            .AnyAsync(r => r.Id == id && r.Ativo);
    }

    public async Task<IEnumerable<Receipt>> ListarPorCategoriaAsync(string categoria, Guid usuarioId)
    {
        return await _context.Receipts
            .Include(r => r.Usuario)
            .Where(r => r.Categoria == categoria && r.UsuarioId == usuarioId && r.Ativo)
            .OrderByDescending(r => r.DataUpload)
            .ToListAsync();
    }
}
