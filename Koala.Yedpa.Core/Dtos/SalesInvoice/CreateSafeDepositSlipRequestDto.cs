using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Koala.Yedpa.Core.Dtos.SalesInvoice;

/// <summary>
/// Logo Go'ya nakit ödemesi için kasa fişi (Safe Deposit Slip) oluşturma isteği DTO'su
/// </summary>
public class CreateSafeDepositSlipRequestDto
{
    /// <summary>
    /// Fiş tipi (11 = Kasa girişi)
    /// </summary>
    [Required]
    [JsonPropertyName("TYPE")]
    public string Type { get; set; } = "11";

    /// <summary>
    /// Kasa kodu
    /// </summary>
    [Required]
    [JsonPropertyName("SD_CODE")]
    public string SdCode { get; set; } = string.Empty;

    /// <summary>
    /// Fiş tarihi (yyyy-MM-dd formatı)
    /// </summary>
    [Required]
    [JsonPropertyName("DATE")]
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Fiş saati (saat kısmı)
    /// </summary>
    [JsonPropertyName("HOUR")]
    public string? Hour { get; set; }

    /// <summary>
    /// Fiş saati (dakika kısmı)
    /// </summary>
    [JsonPropertyName("MINUTE")]
    public string? Minute { get; set; }

    /// <summary>
    /// Fiş numarası (boş bırakılırsa Logo otomatik atar)
    /// </summary>
    [JsonPropertyName("NUMBER")]
    public string? Number { get; set; } = "~";

    /// <summary>
    /// Fiş açıklaması
    /// </summary>
    [JsonPropertyName("DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// Toplam tutar
    /// </summary>
    [Required]
    [JsonPropertyName("AMOUNT")]
    public double Amount { get; set; }

    /// <summary>
    /// Döviz kuru (varsayılan 1)
    /// </summary>
    [Required]
    [JsonPropertyName("TC_XRATE")]
    public string TcXrate { get; set; } = "1";

    /// <summary>
    /// Belge tarihi (yyyy-MM-dd formatı)
    /// </summary>
    [JsonPropertyName("DOC_DATE")]
    public string? DocDate { get; set; }

    /// <summary>
    /// Fiş saati (Logo time formatı)
    /// </summary>
    [JsonPropertyName("TIME")]
    public string? Time { get; set; }

    /// <summary>
    /// Cari hesap ekleri
    /// </summary>
    [JsonPropertyName("ATTACHMENT_ARP")]
    public List<SafeDepositSlipAttachmentArpDto>? AttachmentArp { get; set; } = new List<SafeDepositSlipAttachmentArpDto>();
}

/// <summary>
/// Kasa fişi cari hesap eki DTO'su
/// </summary>
public class SafeDepositSlipAttachmentArpDto
{
    /// <summary>
    /// Cari hesap kodu
    /// </summary>
    [Required]
    [JsonPropertyName("ARP_CODE")]
    public string ArpCode { get; set; } = string.Empty;

    /// <summary>
    /// Ek açıklaması
    /// </summary>
    [JsonPropertyName("DESCRIPTION")]
    public string? Description { get; set; }

    /// <summary>
    /// Alacak tutarı
    /// </summary>
    [Required]
    [JsonPropertyName("CREDIT")]
    public double Credit { get; set; }

    /// <summary>
    /// Döviz kuru (varsayılan 1)
    /// </summary>
    [Required]
    [JsonPropertyName("TC_XRATE")]
    public string TcXrate { get; set; } = "1";

    /// <summary>
    /// Dövizli tutar
    /// </summary>
    [Required]
    [JsonPropertyName("TC_AMOUNT")]
    public double TcAmount { get; set; }

    /// <summary>
    /// Ödeme planı
    /// </summary>
    [Required]
    [JsonPropertyName("PAYMENT_LIST")]
    public List<SafeDepositSlipPaymentDto> PaymentList { get; set; } = new List<SafeDepositSlipPaymentDto>();

    /// <summary>
    /// Ay
    /// </summary>
    [Required]
    [JsonPropertyName("MONTH")]
    public string Month { get; set; } = string.Empty;

    /// <summary>
    /// Yıl
    /// </summary>
    [Required]
    [JsonPropertyName("YEAR")]
    public string Year { get; set; } = string.Empty;

    /// <summary>
    /// Belge tarihi (yyyy-MM-dd formatı)
    /// </summary>
    [JsonPropertyName("DOC_DATE")]
    public string? DocDate { get; set; }
}

/// <summary>
/// Kasa fişi ödeme planı DTO'su
/// </summary>
public class SafeDepositSlipPaymentDto
{
    /// <summary>
    /// Ödeme tarihi (yyyy-MM-dd formatı)
    /// </summary>
    [Required]
    [JsonPropertyName("DATE")]
    public string Date { get; set; } = string.Empty;

    /// <summary>
    /// Modül numarası (10 = Kasa)
    /// </summary>
    [Required]
    [JsonPropertyName("MODULENR")]
    public string ModuleNr { get; set; } = "10";

    /// <summary>
    /// İşaret (1 = Borç/Alacak)
    /// </summary>
    [Required]
    [JsonPropertyName("SIGN")]
    public string Sign { get; set; } = "1";

    /// <summary>
    /// İşlem kodu (1 = Kasa girişi)
    /// </summary>
    [Required]
    [JsonPropertyName("TRCODE")]
    public string TrCode { get; set; } = "1";

    /// <summary>
    /// Toplam tutar
    /// </summary>
    [Required]
    [JsonPropertyName("TOTAL")]
    public double Total { get; set; }

    /// <summary>
    /// İşlem tarihi (yyyy-MM-dd formatı)
    /// </summary>
    [Required]
    [JsonPropertyName("PROCDATE")]
    public string ProcDate { get; set; } = string.Empty;

    /// <summary>
    /// İşlem kuru (varsayılan 1)
    /// </summary>
    [Required]
    [JsonPropertyName("TRRATE")]
    public string TrRate { get; set; } = "1";

    /// <summary>
    /// İskonto vade tarihi (yyyy-MM-dd formatı)
    /// </summary>
    [Required]
    [JsonPropertyName("DISCOUNT_DUEDATE")]
    public string DiscountDuedate { get; set; } = string.Empty;

    /// <summary>
    /// Ödeme sırası
    /// </summary>
    [Required]
    [JsonPropertyName("PAY_NO")]
    public string PayNo { get; set; } = "1";

    /// <summary>
    /// Satır açıklaması
    /// </summary>
    [JsonPropertyName("LINE_EXP")]
    public string? LineExp { get; set; }
}
