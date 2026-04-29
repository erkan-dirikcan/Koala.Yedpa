using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories.Yonetim;

/// <summary>
/// Sözleşme Repository Implementation
/// </summary>
public class SozlesmeRepository : ISozlesmeRepository
{
    private readonly AppDbContext _context;

    public SozlesmeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SozlesmeListDto>> GetAllAsync()
    {
        return await _context.Sozlesmeler
            .Where(s => s.Status != StatusEnum.Deleted)
            .Select(s => new SozlesmeListDto
            {
                SozlesmeID = s.SozlesmeID,
                Firma = s.Firma,
                Konu = s.Konu,
                Baslangic = s.Baslangic,
                Bitis = s.Bitis,
                Durum = s.Bitti ? "Bitmiş" : (s.AzKalda ? "Yaklaşan" : "Aktif"),
                KalanGun = s.Bitis > DateTime.Now ? (s.Bitis - DateTime.Now).Days : 0
            })
            .OrderByDescending(s => s.Baslangic)
            .ToListAsync();
    }

    public async Task<Sozlesme?> GetByIdAsync(int id)
    {
        return await _context.Sozlesmeler
            .Include(s => s.IlgiliKisiler)
            .ThenInclude(k => k.Mail)
            .FirstOrDefaultAsync(s => s.SozlesmeID == id);
    }

    public async Task<List<SozlesmeListDto>> GetExpiringContractsAsync(int days)
    {
        var targetDate = DateTime.Now.AddDays(days);
        return await _context.Sozlesmeler
            .Where(s => s.Status != StatusEnum.Deleted
                && !s.Bitti
                && s.Bitis <= targetDate
                && s.Bitis > DateTime.Now)
            .Select(s => new SozlesmeListDto
            {
                SozlesmeID = s.SozlesmeID,
                Firma = s.Firma,
                Konu = s.Konu,
                Baslangic = s.Baslangic,
                Bitis = s.Bitis,
                Durum = "Yaklaşan",
                KalanGun = (s.Bitis - DateTime.Now).Days
            })
            .OrderBy(s => s.Bitis)
            .ToListAsync();
    }

    public async Task<bool> CreateAsync(Sozlesme sozlesme)
    {
        sozlesme.CreateTime = DateTime.UtcNow;
        sozlesme.Status = StatusEnum.Active;
        sozlesme.Bitti = false;
        sozlesme.Arsiv = false;
        _context.Sozlesmeler.Add(sozlesme);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(Sozlesme sozlesme)
    {
        var existing = await _context.Sozlesmeler.FindAsync(sozlesme.SozlesmeID);
        if (existing == null) return false;

        existing.Firma = sozlesme.Firma;
        existing.Konu = sozlesme.Konu;
        existing.Tur = sozlesme.Tur;
        existing.Baslangic = sozlesme.Baslangic;
        existing.Bitis = sozlesme.Bitis;
        existing.Birim = sozlesme.Birim;
        existing.Pdf = sozlesme.Pdf;
        existing.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var sozlesme = await _context.Sozlesmeler.FindAsync(id);
        if (sozlesme == null) return false;

        sozlesme.Status = StatusEnum.Deleted;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<byte[]?> GetContractPdfAsync(int sozlesmeId)
    {
        return await _context.Sozlesmeler
            .Where(s => s.SozlesmeID == sozlesmeId)
            .Select(s => s.Pdf)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> UpdateContractStatusAsync(int id, bool bitti, string? sonKisi)
    {
        var sozlesme = await _context.Sozlesmeler.FindAsync(id);
        if (sozlesme == null) return false;

        sozlesme.Bitti = bitti;
        sozlesme.SonKisi = sonKisi;
        sozlesme.SonTarih = bitti ? DateTime.Now : null;
        sozlesme.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddIlgiliKisiAsync(SozlesmeKisi kisi)
    {
        _context.SozlesmeKisiler.Add(kisi);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveIlgiliKisiAsync(int kisiId)
    {
        var kisi = await _context.SozlesmeKisiler.FindAsync(kisiId);
        if (kisi == null) return false;

        _context.SozlesmeKisiler.Remove(kisi);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<SozlesmeKisi>> GetIlgiliKisilerAsync(int sozlesmeId)
    {
        return await _context.SozlesmeKisiler
            .Include(k => k.Mail)
            .Where(k => k.SozlesmeID == sozlesmeId)
            .ToListAsync();
    }
}
