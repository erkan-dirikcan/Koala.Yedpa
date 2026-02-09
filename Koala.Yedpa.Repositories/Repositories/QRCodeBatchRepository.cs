using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class QRCodeBatchRepository : IQRCodeBatchRepository
{
    private readonly DbSet<QRCodeBatch> _dbSet;
    private readonly AppDbContext _context;

    public QRCodeBatchRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<QRCodeBatch>();
    }

    public async Task<IEnumerable<QRCodeBatch>> GetAllAsync()
    {
        return await _dbSet
            .OrderByDescending(b => b.CreateTime)
            .ToListAsync();
    }

    public async Task<QRCodeBatch?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<QRCodeBatch>> GetByYearAsync(string year)
    {
        return await _dbSet
            .Where(b => b.Status != StatusEnum.Deleted)
            .Where(b => b.QrCodeYear == year)
            .OrderByDescending(b => b.CreateTime)
            .ToListAsync();
    }

    public async Task<QRCodeBatch> AddAsync(QRCodeBatch entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public QRCodeBatch Update(QRCodeBatch entity)
    {
        var entry = _dbSet.Update(entity);
        return entry.Entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            entity.Status = StatusEnum.Deleted;
            entity.LastUpdateTime = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet
            .Where(b => b.Status != StatusEnum.Deleted)
            .CountAsync();
    }
}
