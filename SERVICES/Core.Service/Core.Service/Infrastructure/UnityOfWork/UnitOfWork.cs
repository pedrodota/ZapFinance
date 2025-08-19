using Core.Service.Infrastructure.Data.DbContext;
using Core.Service.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Core.Service.Infrastructure.UnityOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly ZapFinanceDbContext _context;
    private IDbContextTransaction? _transaction;
    private IUsuarioRepository? _usuarioRepository;

    public UnitOfWork(ZapFinanceDbContext context)
    {
        _context = context;
    }

    public IUsuarioRepository Usuarios => 
        _usuarioRepository ??= new UsuarioRepository(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
