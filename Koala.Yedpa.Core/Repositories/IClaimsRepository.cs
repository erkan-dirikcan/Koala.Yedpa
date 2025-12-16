using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories;

public interface IClaimsRepository
{
    Task<List<Claims>?> GetClaimsByModuleIdAsync(string moduleId);
    Task<List<Claims>?> GetAllClaimsAsync();
    Task<List<Claims>?> WhereClaimsAsync(Expression<Func<Claims, bool>> predicate);
    Task<Claims?> GetClaimByIdAsync(string id);
    Task AddClaimAsync(Claims claim);
    Claims UpdateClaim(Claims claim);
    Task DeleteClaimAsync(string id);
}