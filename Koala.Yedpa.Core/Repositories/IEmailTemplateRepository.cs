using Koala.Yedpa.Core.Models;
using System.Linq.Expressions;

namespace Koala.Yedpa.Core.Repositories;

public interface IEmailTemplateRepository
{
    Task<IEnumerable<EmailTemplate>> GetAllAsync();
    IQueryable<EmailTemplate> WhereAsync(Expression<Func<EmailTemplate, bool>> predicate);
    Task<EmailTemplate> GetByIdAsync(string id);
    Task<EmailTemplate> GetByNameAsyc(string name);
    EmailTemplate Update(EmailTemplate emailTemplate);
    Task AddAsync(EmailTemplate emailTemplate);
    void DeleteAsync(EmailTemplate entity);
    Task<bool> IsExistAsync(string name);
    

}