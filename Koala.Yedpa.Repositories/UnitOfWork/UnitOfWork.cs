using Koala.Yedpa.Core.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.UnitOfWork;

public class UnitOfWork<TContext>(TContext context) : IUnitOfWork<TContext>
    where TContext : DbContext
{
    public void Commit()
    {
        context.SaveChanges();
    }

    public async Task CommitAsync()
    {
        try
        {
            var x = await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {

            throw new Exception(ex.Message);
        }

    }

    public void Dispose()
    {
        context.Dispose();
    }
}