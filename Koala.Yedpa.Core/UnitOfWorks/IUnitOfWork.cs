using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Core.UnitOfWorks;

public interface IUnitOfWork<TContext> : IDisposable where TContext : DbContext
{
    Task CommitAsync();
    void Commit();
}