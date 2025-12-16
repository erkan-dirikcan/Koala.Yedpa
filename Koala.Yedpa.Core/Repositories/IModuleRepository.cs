using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories;

public interface IModuleRepository
{
    Task<Module> GetModuleByIdAsync(string id);
    Task<IEnumerable<Module>> GetAllModuleAsync();
    IQueryable<Module> WhereModule(Expression<Func<Module, bool>> predicate);
    Task AddModuleAsync(Module entity);
    void DeleteModule(Module entity);
    Module UpdateModule(Module entity);
    Task<List<Module>> GetModuleWithExtentedProperty();
}