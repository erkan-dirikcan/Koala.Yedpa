using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories;

public interface IGeneratedIdsRepository
{
    Task<List<GeneratedIds>> GetAllGeneratedIdsAsync();
    IQueryable<GeneratedIds> Where(Expression<Func<GeneratedIds, bool>> predicate);
    Task<GeneratedIds?> GetGeneratedIdByIdAsync(string id);
    Task AddGeneratedIdAsync(GeneratedIds model);
    void UpdateGeneratedId(GeneratedIds model);

}