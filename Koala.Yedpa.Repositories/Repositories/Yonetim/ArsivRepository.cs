using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories.Yonetim;

/// <summary>
/// Arşiv Repository Implementation
/// </summary>
public class ArsivRepository : IArsivRepository
{
    private readonly AppDbContext _context;

    public ArsivRepository(AppDbContext context)
    {
        _context = context;
    }

    // Raf işlemleri
    public async Task<List<Raf>> GetAllRaflarAsync()
    {
        return await _context.Raflar
            .Include(r => r.Bolumeler)
            .ThenInclude(b => b.Koliler)
            .OrderBy(r => r.RafKod)
            .ToListAsync();
    }

    public async Task<Raf?> GetRafByIdAsync(int id)
    {
        return await _context.Raflar
            .Include(r => r.Bolumeler)
            .ThenInclude(b => b.Koliler)
            .FirstOrDefaultAsync(r => r.RafID == id);
    }

    public async Task<bool> AddRafAsync(Raf raf)
    {
        raf.CreateTime = DateTime.UtcNow;
        raf.Status = StatusEnum.Active;
        _context.Raflar.Add(raf);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateRafAsync(Raf raf)
    {
        var existing = await _context.Raflar.FindAsync(raf.RafID);
        if (existing == null) return false;

        existing.RafKod = raf.RafKod;
        existing.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteRafAsync(int id)
    {
        var raf = await _context.Raflar.FindAsync(id);
        if (raf == null) return false;

        raf.Status = StatusEnum.Deleted;
        await _context.SaveChangesAsync();
        return true;
    }

    // Bölme işlemleri
    public async Task<List<Bolme>> GetBolumelerByRafAsync(int rafId)
    {
        return await _context.Bolumeler
            .Include(b => b.Koliler)
            .Where(b => b.RafID == rafId && b.Status != StatusEnum.Deleted)
            .OrderBy(b => b.BolmeNo)
            .ToListAsync();
    }

    public async Task<Bolme?> GetBolmeByIdAsync(int id)
    {
        return await _context.Bolumeler
            .Include(b => b.Koliler)
            .FirstOrDefaultAsync(b => b.BolmeID == id);
    }

    public async Task<bool> AddBolmeAsync(Bolme bolme)
    {
        bolme.CreateTime = DateTime.UtcNow;
        bolme.Status = StatusEnum.Active;
        _context.Bolumeler.Add(bolme);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateBolmeAsync(Bolme bolme)
    {
        var existing = await _context.Bolumeler.FindAsync(bolme.BolmeID);
        if (existing == null) return false;

        existing.BolmeNo = bolme.BolmeNo;
        existing.RafID = bolme.RafID;
        existing.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteBolmeAsync(int id)
    {
        var bolme = await _context.Bolumeler.FindAsync(id);
        if (bolme == null) return false;

        bolme.Status = StatusEnum.Deleted;
        await _context.SaveChangesAsync();
        return true;
    }

    // Koli işlemleri
    public async Task<List<Koli>> GetKolilerByRafAsync(int rafId)
    {
        return await _context.Koliler
            .Include(k => k.Bolme)
            .Where(k => k.Bolme.RafID == rafId && k.Status != StatusEnum.Deleted)
            .OrderBy(k => k.KoliNo)
            .ToListAsync();
    }

    public async Task<List<Koli>> GetKolilerByBolmeAsync(int bolmeId)
    {
        return await _context.Koliler
            .Where(k => k.BolmeID == bolmeId && k.Status != StatusEnum.Deleted)
            .OrderBy(k => k.KoliNo)
            .ToListAsync();
    }

    public async Task<Koli?> GetKoliByIdAsync(int id)
    {
        return await _context.Koliler
            .Include(k => k.Bolme)
            .ThenInclude(b => b.Raf)
            .FirstOrDefaultAsync(k => k.KoliID == id);
    }

    public async Task<bool> AddKoliAsync(Koli koli)
    {
        koli.CreateTime = DateTime.UtcNow;
        koli.Status = StatusEnum.Active;
        _context.Koliler.Add(koli);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateKoliAsync(Koli koli)
    {
        var existing = await _context.Koliler.FindAsync(koli.KoliID);
        if (existing == null) return false;

        existing.KoliNo = koli.KoliNo;
        existing.BolmeID = koli.BolmeID;
        existing.Detay = koli.Detay;
        existing.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteKoliAsync(int id)
    {
        var koli = await _context.Koliler.FindAsync(id);
        if (koli == null) return false;

        koli.Status = StatusEnum.Deleted;
        await _context.SaveChangesAsync();
        return true;
    }

    // Arşiv listesi
    public async Task<List<ArsivDto>> GetArsivListesiAsync()
    {
        return await (from r in _context.Raflar
                      join b in _context.Bolumeler on r.RafID equals b.RafID
                      join k in _context.Koliler on b.BolmeID equals k.BolmeID
                      where r.Status != StatusEnum.Deleted
                        && b.Status != StatusEnum.Deleted
                        && k.Status != StatusEnum.Deleted
                      select new ArsivDto
                      {
                          RafID = r.RafID,
                          RafKod = r.RafKod,
                          BolmeID = b.BolmeID,
                          BolmeNo = b.BolmeNo,
                          KoliID = k.KoliID,
                          KoliNo = k.KoliNo,
                          Detay = k.Detay
                      })
                      .OrderBy(x => x.RafKod)
                      .ThenBy(x => x.BolmeNo)
                      .ThenBy(x => x.KoliNo)
                      .ToListAsync();
    }

    public async Task<ArsivDetayDto?> GetKoliDetayAsync(int koliId)
    {
        return await (from r in _context.Raflar
                      join b in _context.Bolumeler on r.RafID equals b.RafID
                      join k in _context.Koliler on b.BolmeID equals k.BolmeID
                      where k.KoliID == koliId
                      select new ArsivDetayDto
                      {
                          RafID = r.RafID,
                          RafKod = r.RafKod,
                          BolmeID = b.BolmeID,
                          BolmeNo = b.BolmeNo,
                          KoliID = k.KoliID,
                          KoliNo = k.KoliNo,
                          Detay = k.Detay,
                          Icerik = string.Empty, // İçerik detayı ihtiyaca göre eklenebilir
                          ToplamEsya = 0 // İçerik sayısı ihtiyaca kadar hesaplanabilir
                      })
                      .FirstOrDefaultAsync();
    }
}
