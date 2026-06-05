using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Koala.Yedpa.Core.Dtos.SalesInvoice;

/// <summary>
/// Logo Go'ya satış faturası oluşturma isteği DTO'su
/// </summary>
public class CreateSalesInvoiceRequestDto
{
    /// <summary>
    /// Grup kodu (genellikle 2)
    /// </summary>
    [Required]
    [JsonPropertyName("GRP_CODE")]
    public int GrpCode { get; set; } = 2;

    /// <summary>
    /// Fatura tipi (9 = Satış Faturası)
    /// </summary>
    [Required]
    [JsonPropertyName("TYPE")]
    public int Type { get; set; } = 9;

    /// <summary>
    /// Fatura tarihi
    /// </summary>
    [Required]
    [JsonPropertyName("DATE")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Fatura numarası (boş bırakılırsa Logo otomatik atar)
    /// </summary>
    [JsonPropertyName("DOC_NUMBER")]
    public string? DocNumber { get; set; }

    /// <summary>
    /// Yetki kodu
    /// </summary>
    [JsonPropertyName("AUTH_CODE")]
    public string? AuthCode { get; set; }

    /// <summary>
    /// Cari hesap kodu
    /// </summary>
    [Required]
    [JsonPropertyName("ARP_CODE")]
    public string ArpCode { get; set; } = string.Empty;

    /// <summary>
    /// Gelir hesabı kodu (muhasebe hesabı)
    /// </summary>
    [Required]
    [JsonPropertyName("GL_CODE")]
    public string GlCode { get; set; } = string.Empty;

    /// <summary>
    /// KDV oranı (varsayılan %20)
    /// </summary>
    [JsonPropertyName("VAT_RATE")]
    public int VatRate { get; set; } = 20;

    /// <summary>
    /// Fatura açıklaması 1
    /// </summary>
    [JsonPropertyName("NOTES1")]
    public string? Notes1 { get; set; }

    /// <summary>
    /// Fatura açıklaması 2
    /// </summary>
    [JsonPropertyName("NOTES2")]
    public string? Notes2 { get; set; }

    /// <summary>
    /// Para birimi seçimi (2 = TL)
    /// </summary>
    [JsonPropertyName("CURRSEL_TOTALS")]
    public int CurrSelTotals { get; set; } = 2;

    /// <summary>
    /// İndirim bölümü 1
    /// </summary>
    [JsonPropertyName("DEDUCTION_PART1")]
    public int DeductionPart1 { get; set; } = 2;

    /// <summary>
    /// İndirim bölümü 2
    /// </summary>
    [JsonPropertyName("DEDUCTION_PART2")]
    public int DeductionPart2 { get; set; } = 3;

    /// <summary>
    /// E-fatura mı? (2 = Hayır)
    /// </summary>
    [JsonPropertyName("EINVOICE")]
    public int EInvoice { get; set; } = 2;

    /// <summary>
    /// Profil ID
    /// </summary>
    [JsonPropertyName("PROFILE_ID")]
    public int ProfileId { get; set; } = 0;

    /// <summary>
    /// Döviz kodu (TL, USD, EUR vb.)
    /// </summary>
    [Required]
    [JsonPropertyName("EDTCURR_GLOBAL_CODE")]
    public string EdtCurrGlobalCode { get; set; } = "TL";

    /// <summary>
    /// Fatura kalemleri
    /// </summary>
    [Required]
    [JsonPropertyName("TRANSACTIONS")]
    public List<SalesInvoiceTransactionDto> Transactions { get; set; } = new List<SalesInvoiceTransactionDto>();

    /// <summary>
    /// Ödeme planı
    /// </summary>
    [Required]
    [JsonPropertyName("PAYMENT_LIST")]
    public List<SalesInvoicePaymentDto> PaymentList { get; set; } = new List<SalesInvoicePaymentDto>();

    #region Ödeme Tipi (Banka Fişi / Kasa Fişi İçin)

    /// <summary>
    /// Ödeme tipi
    /// </summary>
    /// <remarks>
    /// Geçerli değerler:
    /// - "CreditCard": Kredi kartı ödemesi (ARP Slip oluşturur)
    /// - "Cash": Nakit ödeme (Kasa fişi oluşturur)
    /// - "Check": Çek ödemesi
    /// - "Bill": Senet ödemesi
    /// - "BankTransfer": Banka havalesi/EFT
    /// - null: Varsayılan davranış (sadece fatura)
    /// </remarks>
    [JsonPropertyName("PAYMENT_TYPE")]
    public string? PaymentType { get; set; }

    /// <summary>
    /// Banka kodu (kredi kartı ödemeleri için)
    /// </summary>
    [JsonPropertyName("BANK_CODE")]
    public string? BankCode { get; set; }

    /// <summary>
    /// Banka hesap kodu (kredi kartı ödemeleri için)
    /// </summary>
    [JsonPropertyName("BANK_ACCOUNT_CODE")]
    public string? BankAccountCode { get; set; }

    #endregion
}

/// <summary>
/// Satış faturası kalem DTO'su
/// </summary>
public class SalesInvoiceTransactionDto
{
    /// <summary>
    /// Kalem tipi (4 = Hizmet satışı)
    /// </summary>
    [Required]
    [JsonPropertyName("TYPE")]
    public int Type { get; set; } = 4;

    /// <summary>
    /// Stok/hizmet kodu
    /// </summary>
    [Required]
    [JsonPropertyName("MASTER_CODE")]
    public string MasterCode { get; set; } = string.Empty;

    /// <summary>
    /// İşlem kodu (9 = Satış)
    /// </summary>
    [Required]
    [JsonPropertyName("TRCODE")]
    public int TrCode { get; set; } = 9;

    /// <summary>
    /// Kalem tarihi
    /// </summary>
    [Required]
    [JsonPropertyName("DATE")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Miktar
    /// </summary>
    [Required]
    [JsonPropertyName("QUANTITY")]
    public decimal Quantity { get; set; } = 1;

    /// <summary>
    /// Birim fiyat
    /// </summary>
    [Required]
    [JsonPropertyName("PRICE")]
    public decimal Price { get; set; }

    /// <summary>
    /// Kalem toplamı
    /// </summary>
    [Required]
    [JsonPropertyName("TOTAL")]
    public decimal Total { get; set; }

    /// <summary>
    /// KDV oranı
    /// </summary>
    [Required]
    [JsonPropertyName("VAT_RATE")]
    public int VatRate { get; set; } = 20;

    /// <summary>
    /// KDV tutarı
    /// </summary>
    [Required]
    [JsonPropertyName("VAT_AMOUNT")]
    public decimal VatAmount { get; set; }

    /// <summary>
    /// KDV matrahı
    /// </summary>
    [Required]
    [JsonPropertyName("VAT_BASE")]
    public decimal VatBase { get; set; }

    /// <summary>
    /// KDV hariç toplam
    /// </summary>
    [Required]
    [JsonPropertyName("TOTAL_NET")]
    public decimal TotalNet { get; set; }

    /// <summary>
    /// Birim kodu (ADET, SAAT vb.)
    /// </summary>
    [Required]
    [JsonPropertyName("UNIT_CODE")]
    public string UnitCode { get; set; } = "ADET";

    /// <summary>
    /// Birim çevrim katsayısı 1
    /// </summary>
    [JsonPropertyName("UNIT_CONV1")]
    public int UnitConv1 { get; set; } = 1;

    /// <summary>
    /// Birim çevrim katsayısı 2
    /// </summary>
    [JsonPropertyName("UNIT_CONV2")]
    public int UnitConv2 { get; set; } = 1;

    /// <summary>
    /// Gelir hesabı kodu 1
    /// </summary>
    [Required]
    [JsonPropertyName("GL_CODE1")]
    public string GlCode1 { get; set; } = string.Empty;

    /// <summary>
    /// Gelir hesabı kodu 2 (KDV hesabı)
    /// </summary>
    [Required]
    [JsonPropertyName("GL_CODE2")]
    public string GlCode2 { get; set; } = string.Empty;

    /// <summary>
    /// Döviz kodu
    /// </summary>
    [Required]
    [JsonPropertyName("EDTCURR_GLOBAL_CODE")]
    public string EdtCurrGlobalCode { get; set; } = "USD";

    /// <summary>
    /// Faturalandı mı? (1 = Evet)
    /// </summary>
    [JsonPropertyName("BILLED")]
    public int Billed { get; set; } = 1;

    /// <summary>
    /// Döviz kuru
    /// </summary>
    [JsonPropertyName("RC_XRATE")]
    public decimal RcXrate { get; set; } = 1;
}

/// <summary>
/// Satış faturası ödeme planı DTO'su
/// </summary>
public class SalesInvoicePaymentDto
{
    /// <summary>
    /// Ödeme tarihi
    /// </summary>
    [Required]
    [JsonPropertyName("DATE")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Modül numarası (4 = Fatura)
    /// </summary>
    [Required]
    [JsonPropertyName("MODULE_NR")]
    public int ModuleNr { get; set; } = 4;

    /// <summary>
    /// İşlem kodu (9 = Satış)
    /// </summary>
    [Required]
    [JsonPropertyName("TRCODE")]
    public int TrCode { get; set; } = 9;

    /// <summary>
    /// Ödeme tutarı
    /// </summary>
    [Required]
    [JsonPropertyName("TOTAL")]
    public decimal Total { get; set; }

    /// <summary>
    /// İşlem tarihi
    /// </summary>
    [Required]
    [JsonPropertyName("PROC_DATE")]
    public DateTime ProcDate { get; set; }

    /// <summary>
    /// Raporlama kuru
    /// </summary>
    [JsonPropertyName("REPORT_RATE")]
    public decimal ReportRate { get; set; } = 1;

    /// <summary>
    /// Ödeme sırası
    /// </summary>
    [Required]
    [JsonPropertyName("PAY_NO")]
    public int PayNo { get; set; } = 1;

    /// <summary>
    /// İskonto vade tarihi
    /// </summary>
    [Required]
    [JsonPropertyName("DISCOUNT_DUE_DATE")]
    public DateTime DiscountDueDate { get; set; }
}
