using System.ComponentModel.DataAnnotations;

namespace Koala.Yedpa.Core.Dtos.KrediKartTahsilat;

/// <summary>
/// Kredi karti tahsilati icin dis katmandan alinan slim input modeli (Model 1).
/// Geri kalan tum degerler (TIME, sabitler) serviste uretilir.
/// NUMBER ve GUID opsiyonel olarak kalici tanimlanabilir; bos gelirse servis uretir.
/// </summary>
public class CreateKrediKartTahsilatRequestDto
{
    /// <summary>Tahsilat tarihi (faturanin degil, tahsilatin tarihi)</summary>
    [Required]
    public DateTime CollectionDate { get; set; }

    /// <summary>
    /// Banka hesap kodu — GET api/KrediKartTahsilat/cards endpoint'inin AccountCode alanından gelir.
    /// Logo REST kodu ile ilgili GL kodlarını FillAccCodesOnPreSave ile otomatik bulur.
    /// Örnek: "11    0015"
    /// </summary>
    [Required]
    public string BankAccCode { get; set; } = string.Empty;

    /// <summary>
    /// Fiş numarası (opsiyonel). Boş/null gelirse servis "~" kullanır (Logo otomatik sıralı no üretir).
    /// Logo'nun FICHENO alanı 16 karakterle sınırlı; daha uzun değer kırpılır ve farklı numaralar
    /// aynı fişe düşüp çakışabilir. Bu yüzden burada peşinen reddediyoruz.
    /// </summary>
    [MaxLength(16, ErrorMessage = "Fiş numarası en fazla 16 karakter olabilir (Logo FICHENO sınırı).")]
    public string? Number { get; set; }

    /// <summary>
    /// İdempotency GUID (opsiyonel). Boş/null gelirse servis Guid.NewGuid() ile üretir.
    /// Tekrar gönderimlerde aynı GUID verilirse Logo çift kayıt oluşturmaz.
    /// </summary>
    public string? Guid { get; set; }

    /// <summary>Fis aciklamasi (opsiyonel)</summary>
    public string? Notes1 { get; set; }

    /// <summary>Tahsilat kalemleri — her kalem bir cari (CustomerCode = ARP_CODE)</summary>
    [Required]
    [MinLength(1)]
    public List<KrediKartTahsilatItemDto> Items { get; set; } = new();
}

/// <summary>
/// Tek bir cari icin tahsilat kalemi.
/// </summary>
public class KrediKartTahsilatItemDto
{
    /// <summary>Cari kodu (PendingInvoices.CustomerCode = ARP_CODE)</summary>
    [Required]
    public string CustomerCode { get; set; } = string.Empty;

    /// <summary>Tahsilat tutari (PendingInvoices.RemainingAmount'a esit)</summary>
    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    /// <summary>Cari adi (opsiyonel — tek cari oldugunda NOTES1 fallback olarak kullanilabilir)</summary>
    public string? CustomerName { get; set; }
}
