using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories.Yonetim;

/// <summary>
/// Arıza Repository Implementation
/// </summary>
public class ArizaRepository : IArizaRepository
{
    private readonly AppDbContext _context;

    public ArizaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ArizaListDto>> GetAllAsync()
    {
        return await _context.Arizalar
            .Where(a => a.Status != StatusEnum.Deleted)
            .Select(a => new ArizaListDto
            {
                ArizaID = a.ArizaID,
                Konu = a.Konu,
                Tarih = a.Tarih,
                Birim = a.Birim,
                Durum = a.Durum,
                SonKisi = a.SonKisi,
                Gizli = a.Gizli,
                HareketSayisi = a.Hareketler.Count
            })
            .OrderByDescending(a => a.Tarih)
            .ToListAsync();
    }

    public async Task<Ariza?> GetByIdAsync(int id)
    {
        return await _context.Arizalar
            .Include(a => a.Hareketler)
            .Include(a => a.IlgiliKisiler)
            .ThenInclude(k => k.Mail)
            .FirstOrDefaultAsync(a => a.ArizaID == id);
    }

    public async Task<List<ArizaListDto>> GetByBirimAsync(string birim)
    {
        return await _context.Arizalar
            .Where(a => a.Status != StatusEnum.Deleted && a.Birim == birim)
            .Select(a => new ArizaListDto
            {
                ArizaID = a.ArizaID,
                Konu = a.Konu,
                Tarih = a.Tarih,
                Birim = a.Birim,
                Durum = a.Durum,
                SonKisi = a.SonKisi,
                Gizli = a.Gizli,
                HareketSayisi = a.Hareketler.Count
            })
            .OrderByDescending(a => a.Tarih)
            .ToListAsync();
    }

    public async Task<List<ArizaListDto>> GetActiveFaultsAsync()
    {
        return await _context.Arizalar
            .Where(a => a.Status != StatusEnum.Deleted && a.Durum != "Tamamlandı")
            .Select(a => new ArizaListDto
            {
                ArizaID = a.ArizaID,
                Konu = a.Konu,
                Tarih = a.Tarih,
                Birim = a.Birim,
                Durum = a.Durum,
                SonKisi = a.SonKisi,
                Gizli = a.Gizli,
                HareketSayisi = a.Hareketler.Count
            })
            .OrderByDescending(a => a.Tarih)
            .ToListAsync();
    }

    public async Task<bool> CreateAsync(Ariza ariza)
    {
        ariza.CreateTime = DateTime.UtcNow;
        ariza.Status = StatusEnum.Active;
        _context.Arizalar.Add(ariza);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(Ariza ariza)
    {
        var existing = await _context.Arizalar.FindAsync(ariza.ArizaID);
        if (existing == null) return false;

        existing.FirmaAdres = ariza.FirmaAdres;
        existing.Konu = ariza.Konu;
        existing.Birim = ariza.Birim;
        existing.Gizli = ariza.Gizli;
        existing.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var ariza = await _context.Arizalar.FindAsync(id);
        if (ariza == null) return false;

        ariza.Status = StatusEnum.Deleted;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateDurumAsync(int id, string durum, string? sonKisi)
    {
        var ariza = await _context.Arizalar.FindAsync(id);
        if (ariza == null) return false;

        ariza.Durum = durum;
        ariza.SonKisi = sonKisi;
        ariza.SonTarih = DateTime.Now;
        ariza.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AddHareketAsync(ArizaHareket hareket)
    {
        hareket.Tarih = DateTime.Now;
        _context.ArizaHareketleri.Add(hareket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ArizaHareket>> GetHareketlerAsync(int arizaId)
    {
        return await _context.ArizaHareketleri
            .Where(h => h.ArizaID == arizaId)
            .OrderByDescending(h => h.Tarih)
            .ToListAsync();
    }

    public async Task<bool> AddIlgiliKisiAsync(ArizaKisi kisi)
    {
        _context.ArizaKisiler.Add(kisi);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveIlgiliKisiAsync(int kisiId)
    {
        var kisi = await _context.ArizaKisiler.FindAsync(kisiId);
        if (kisi == null) return false;

        _context.ArizaKisiler.Remove(kisi);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<ArizaKisi>> GetIlgiliKisilerAsync(int arizaId)
    {
        return await _context.ArizaKisiler
            .Include(k => k.Mail)
            .Where(k => k.ArizaID == arizaId)
            .ToListAsync();
    }
}
