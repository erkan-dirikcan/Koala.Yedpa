using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories
{
    public interface ISiteRepository
    {
        Task<IEnumerable<LgXt001211>> GetAllAsync();
        IQueryable<LgXt001211> Where(Expression<Func<LgXt001211, bool>> predicate);
        Task<LgXt001211> GetByIdAsync(string id);
        Task<LgXt001211> GetByLogRefAsyc(int logReg);
        Task<LgXt001211> GetByParLogRefAsyc(int parLogRef);
        Task<LgXt001211> GetByCodeAsyc(string code);
        LgXt001211 Update(LgXt001211 emailTemplate);
        Task<int> CountAsync();
        Task<int> CountAsync(Expression<Func<LgXt001211, bool>> predicate);
        Task<IEnumerable<LgXt001211>> GetPagedAsync(int skip, int take, Expression<Func<LgXt001211, bool>>? predicate = null, string? orderBy = null, bool ascending = true);

    }
}
