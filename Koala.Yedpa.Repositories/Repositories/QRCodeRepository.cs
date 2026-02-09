using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Models;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories;

public class QRCodeRepository : IQRCodeRepository
{
    private readonly DbSet<QRCode> _dbSet;
    private readonly AppDbContext _context;

    public QRCodeRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<QRCode>();
    }

    public async Task<IEnumerable<QRCode>> GetAllAsync()
    {
        return await _dbSet
            .Include(q => q.Batch)
            .OrderByDescending(q => q.CreateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<QRCode>> GetByBatchIdAsync(int batchId)
    {
        return await _dbSet
            .Where(q => q.BatchId == batchId)
            .OrderByDescending(q => q.CreateTime)
            .ToListAsync();
    }

    public async Task<QRCode?> GetByPartnerNoAsync(string partnerNo)
    {
        return await _dbSet
            .Where(q => q.Status != StatusEnum.Deleted)
            .FirstOrDefaultAsync(q => q.PartnerNo == partnerNo);
    }

    public async Task<IEnumerable<QRCode>> GetByYearAsync(string year)
    {
        return await _dbSet
            .Where(q => q.Status != StatusEnum.Deleted)
            .Where(q => q.QrCodeYear == year)
            .OrderByDescending(q => q.CreateTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<QRCode>> GetByStatusAsync(StatusEnum status)
    {
        return await _dbSet
            .Where(q => q.Status == status)
            .OrderByDescending(q => q.CreateTime)
            .ToListAsync();
    }

    public async Task<QRCode> AddAsync(QRCode entity)
    {
        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public QRCode Update(QRCode entity)
    {
        var entry = _dbSet.Update(entity);
        return entry.Entity;
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _dbSet.FindAsync(id);
        if (entity != null)
        {
            // Soft delete
            entity.Status = StatusEnum.Deleted;
            entity.LastUpdateTime = DateTime.UtcNow;
            _dbSet.Update(entity);
        }
    }

    public async Task DeleteAllAsync()
    {
        // Hard delete - tüm kayıtları fiziksel olarak sil
        var allRecords = await _dbSet.ToListAsync();
        _dbSet.RemoveRange(allRecords);
    }

    public async Task DeleteByYearAsync(string year)
    {
        var records = await _dbSet
            .Where(q => q.QrCodeYear == year)
            .ToListAsync();
        _dbSet.RemoveRange(records);
    }

    public async Task<int> CountAsync()
    {
        return await _dbSet
            .Where(q => q.Status != StatusEnum.Deleted)
            .CountAsync();
    }

    public async Task<int> CountAsync(StatusEnum status)
    {
        return await _dbSet
            .Where(q => q.Status == status)
            .CountAsync();
    }
}
