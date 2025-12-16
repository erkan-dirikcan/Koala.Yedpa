using System.Linq.Expressions;
using Koala.Yedpa.Core.Models;

namespace Koala.Yedpa.Core.Repositories;

public interface IExtendedPropertiesRepository
{
    Task<List<ExtendedProperties>> GetAllExtendedPropertiesAsync();
    Task<List<ExtendedProperties>> GetModuleExtendedPropertiesAsync(string moduleId);
    IQueryable<ExtendedProperties> WhereExtendedPropertiesAsync(Expression<Func<ExtendedProperties, bool>> predicate);

    Task CreateExtendedProperties(ExtendedProperties model);
    void UpdateExtendedProperties(ExtendedProperties model);
    Task<ExtendedProperties?> GetExtendedPropertiesByIdAsync(string id);
    Task<ExtendedProperties?> GetExtendedPropertiesByNameAsync(string name);
}