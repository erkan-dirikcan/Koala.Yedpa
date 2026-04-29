using Koala.Yedpa.Core.Dtos;
using Koala.Yedpa.Core.Dtos.Yonetim;
using Koala.Yedpa.Core.Models.Yonetim;
using Koala.Yedpa.Core.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Koala.Yedpa.Repositories.Repositories.Yonetim;

/// <summary>
/// Otopark Repository Implementation
/// </summary>
public class OtoparkRepository : IOtoparkRepository
{
    private readonly AppDbContext _context;

    public OtoparkRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OtoparkListDto>> GetAllAsync()
    {
        return await _context.OtoparkKayitlari
            .Where(k => k.Status != StatusEnum.Deleted)
            .OrderByDescending(k => k.GirisTarih)
            .Select(k => new OtoparkListDto
            {
                KayitID = k.KayitID,
                Plaka = k.Plaka,
                GirisTarih = k.GirisTarih,
                CikisTarih = k.CikisTarih,
                AboneAd = k.AboneAd,
                Telefon = k.Telefon
            })
            .ToListAsync();
    }

    public async Task<OtoparkKayit?> GetByIdAsync(int id)
    {
        return await _context.OtoparkKayitlari
            .FirstOrDefaultAsync(k => k.KayitID == id);
    }

    public async Task<OtoparkKayit?> GetByPlakaAsync(string plaka)
    {
        return await _context.OtoparkKayitlari
            .Where(k => k.Plaka == plaka && k.Status != StatusEnum.Deleted)
            .OrderByDescending(k => k.GirisTarih)
            .FirstOrDefaultAsync();
    }

    public async Task<List<OtoparkListDto>> GetActiveSubscriptionsAsync()
    {
        var today = DateTime.Now;
        return await _context.OtoparkKayitlari
            .Where(k => k.Status != StatusEnum.Deleted
                && k.CikisTarih == null
                && k.GirisTarih.HasValue)
            .OrderByDescending(k => k.GirisTarih)
            .Select(k => new OtoparkListDto
            {
                KayitID = k.KayitID,
                Plaka = k.Plaka,
                GirisTarih = k.GirisTarih,
                CikisTarih = k.CikisTarih,
                AboneAd = k.AboneAd,
                Telefon = k.Telefon
            })
            .ToListAsync();
    }

    public async Task<bool> GirisYapAsync(OtoparkKayit kayit)
    {
        kayit.GirisTarih = DateTime.Now;
        kayit.CikisTarih = null;
        kayit.CreateTime = DateTime.UtcNow;
        kayit.Status = StatusEnum.Active;
        _context.OtoparkKayitlari.Add(kayit);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> CikisYapAsync(string plaka)
    {
        var kayit = await _context.OtoparkKayitlari
            .Where(k => k.Plaka == plaka && k.CikisTarih == null)
            .OrderByDescending(k => k.GirisTarih)
            .FirstOrDefaultAsync();

        if (kayit == null) return false;

        kayit.CikisTarih = DateTime.Now;
        kayit.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AboneEkleAsync(OtoparkKayit kayit)
    {
        kayit.GirisTarih = DateTime.Now;
        kayit.CikisTarih = null; // Abonelik için çıkış tarihi boş
        kayit.CreateTime = DateTime.UtcNow;
        kayit.Status = StatusEnum.Active;
        _context.OtoparkKayitlari.Add(kayit);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AboneGuncelleAsync(OtoparkKayit kayit)
    {
        var existing = await _context.OtoparkKayitlari.FindAsync(kayit.KayitID);
        if (existing == null) return false;

        existing.Plaka = kayit.Plaka;
        existing.AboneAd = kayit.AboneAd;
        existing.Telefon = kayit.Telefon;
        existing.LastUpdateTime = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AboneSilAsync(int id)
    {
        var kayit = await _context.OtoparkKayitlari.FindAsync(id);
        if (kayit == null) return false;

        kayit.Status = StatusEnum.Deleted;
        await _context.SaveChangesAsync();
        return true;
    }
}

/// <summary>
/// Ortak Repository Implementation
/// </summary>
public class OrtakRepository : IOrtakRepository
{
    private readonly AppDbContext _context;

    public OrtakRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Birim>> GetAllBirimlerAsync()
    {
        return await _context.Birimler
            .OrderBy(b => b.BirimAdi)
            .ToListAsync();
    }

    public async Task<List<Mail>> GetAllMailAdresleriAsync()
    {
        return await _context.MailAdresleri
            .OrderBy(m => m.Ad)
            .ThenBy(m => m.Soyad)
            .ToListAsync();
    }

    public async Task<Mail?> GetMailByIdAsync(int id)
    {
        return await _context.MailAdresleri
            .FirstOrDefaultAsync(m => m.MailID == id);
    }

    public async Task<bool> AddMailAsync(Mail mail)
    {
        _context.MailAdresleri.Add(mail);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateMailAsync(Mail mail)
    {
        var existing = await _context.MailAdresleri.FindAsync(mail.MailID);
        if (existing == null) return false;

        existing.Ad = mail.Ad;
        existing.Soyad = mail.Soyad;
        existing.EPosta = mail.EPosta;
        existing.GSM = mail.GSM;
        existing.Telefon = mail.Telefon;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteMailAsync(int id)
    {
        var mail = await _context.MailAdresleri.FindAsync(id);
        if (mail == null) return false;

        _context.MailAdresleri.Remove(mail);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Durum>> GetAllDurumlarAsync()
    {
        return await _context.Durumlar
            .OrderBy(d => d.DurumAdi)
            .ToListAsync();
    }
}
