using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Koala.Yedpa.Core.Dtos.SalesInvoice;

/// <summary>
/// Logo Go'ya kredi kartı ödemesi için cari hesap fişi (ARP Slip) oluşturma isteği DTO'su
/// </summary>
public class CreateArpSlipRequestDto
{
    /// <summary>
    /// Fiş numarası (boş bırakılırsa Logo otomatik atar)
    /// </summary>
    [JsonPropertyName("NUMBER")]
    public string? Number { get; set; } = "~";

    /// <summary>
    /// Fiş tarihi
    /// </summary>
    [Required]
    [JsonPropertyName("DATE")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Fiş tipi (70 = Kredi kartı ödemesi)
    /// </summary>
    [Required]
    [JsonPropertyName("TYPE")]
    public int Type { get; set; } = 70;

    /// <summary>
    /// Fiş açıklaması 1
    /// </summary>
    [JsonPropertyName("NOTES1")]
    public string? Notes1 { get; set; }

    /// <summary>
    /// Para birimi seçimi (1 = Dövizli, 2 = TL)
    /// </summary>
    [Required]
    [JsonPropertyName("CURRSEL_TOTALS")]
    public int CurrSelTotals { get; set; } = 1;

    /// <summary>
    /// XML özniteliği (2 = Standart)
    /// </summary>
    [Required]
    [JsonPropertyName("XML_ATTRIBUTE")]
    public int XmlAttribute { get; set; } = 2;

    /// <summary>
    /// Fiş saati (Logo time formatı - örn: 143000 için 14:30:00)
    /// </summary>
    [Required]
    [JsonPropertyName("TIME")]
    public int Time { get; set; }

    /// <summary>
    /// Cari hesap işlemleri
    /// </summary>
    [Required]
    [JsonPropertyName("TRANSACTIONS")]
    public List<ArpSlipTransactionDto> Transactions { get; set; } = new List<ArpSlipTransactionDto>();
}

/// <summary>
/// Cari hesap fişi kalem DTO'su
/// </summary>
public class ArpSlipTransactionDto
{
    /// <summary>
    /// Cari hesap kodu
    /// </summary>
    [Required]
    [JsonPropertyName("ARP_CODE")]
    public string ArpCode { get; set; } = string.Empty;

    /// <summary>
    /// Kalem tarihi
    /// </summary>
    [Required]
    [JsonPropertyName("DATE")]
    public DateTime Date { get; set; }

    /// <summary>
    /// Modül numarası (5 = Cari hesap)
    /// </summary>
    [Required]
    [JsonPropertyName("MODULENR")]
    public int ModuleNr { get; set; } = 5;

    /// <summary>
    /// İşlem kodu (70 = Kredi kartı ödemesi)
    /// </summary>
    [Required]
    [JsonPropertyName("TRCODE")]
    public int TrCode { get; set; } = 70;

    /// <summary>
    /// Belge numarası
    /// </summary>
    [JsonPropertyName("DOC_NUMBER")]
    public string? DocNumber { get; set; }

    /// <summary>
    /// Alacak tutarı
    /// </summary>
    [Required]
    [JsonPropertyName("CREDIT")]
    public double Credit { get; set; }

    /// <summary>
    /// İşaret (1 = Alacak)
    /// </summary>
    [Required]
    [JsonPropertyName("SIGN")]
    public int Sign { get; set; } = 1;

    /// <summary>
    /// Tutar
    /// </summary>
    [Required]
    [JsonPropertyName("AMOUNT")]
    public double Amount { get; set; }

    /// <summary>
    /// Dövizli tutar
    /// </summary>
    [Required]
    [JsonPropertyName("TC_AMOUNT")]
    public double TcAmount { get; set; }

    /// <summary>
    /// XML özniteliği (2 = Standart)
    /// </summary>
    [Required]
    [JsonPropertyName("XML_ATTRIBUTE")]
    public int XmlAttribute { get; set; } = 2;

    /// <summary>
    /// Ay (kredi kartı ekstresi ayı)
    /// </summary>
    [Required]
    [JsonPropertyName("MONTH")]
    public int Month { get; set; }

    /// <summary>
    /// Yıl (kredi kartı ekstresi yılı)
    /// </summary>
    [Required]
    [JsonPropertyName("YEAR")]
    public int Year { get; set; }

    /// <summary>
    /// Banka hesap referansı (1 = Banka hesabı)
    /// </summary>
    [Required]
    [JsonPropertyName("BANKACCREF")]
    public int BankAccRef { get; set; } = 1;

    /// <summary>
    /// Banka hesap kodu
    /// </summary>
    [Required]
    [JsonPropertyName("BANKACC_CODE")]
    public string BankAccCode { get; set; } = string.Empty;
}
