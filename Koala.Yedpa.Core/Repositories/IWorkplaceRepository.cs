using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories;

public interface IWorkplaceRepository
{
    Task<IEnumerable<Workplace>> GetAllAsync();
    IQueryable<Workplace> Where(Expression<Func<Workplace, bool>> predicate);
    Task<Workplace?> GetByIdAsync(string id);
    Task<Workplace?> GetByCodeAsync(string code);
    Task<Workplace?> GetByLogicalRefAsync(int logicalRef);
    Task<Workplace?> GetByLogRefAsync(int logRef);
    Task<List<int>> Select(Expression<Func<Workplace, int>> selector);
    Workplace Update(Workplace entity);
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<Workplace, bool>> predicate);
    Task<IEnumerable<Workplace>> GetPagedAsync(int skip, int take, Expression<Func<Workplace, bool>>? predicate = null, string? orderBy = null, bool ascending = true);
}
