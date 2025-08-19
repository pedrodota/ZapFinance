using Core.Service.Infrastructure.Data.Repositories;

namespace Core.Service.Infrastructure.UnityOfWork;

public interface IUnitOfWork : IDisposable
{
    IUsuarioRepository Usuarios { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
