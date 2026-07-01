using Newtonsoft.Json;

namespace Koala.Yedpa.Core.Dtos.KrediKartTahsilat;

/// <summary>
/// Logo REST ArpSlips endpoint'ine gönderilen payload modeli (Model 2).
/// Servis Newtonsoft.Json ile serialize ettiği için [JsonProperty] (Newtonsoft) kullanılır.
/// TRANSACTIONS ve PAYMENT_LIST Logo formatında { "items": [...] } objesi olarak serialize edilir.
/// Sabit (constant) alanlar default değerlerle gömülüdür; mapping sırasında tekrar set edilir.
/// GL kodları (BANK_GL_CODE) Logo tarafında FillAccCodesOnPreSave ile otomatik doldurulur —
/// payload'a eklenmez.
/// </summary>
public class KrediKartTahsilatLogoDto
{
    [JsonProperty("NUMBER")] public string? Number { get; set; } = "~";
    [JsonProperty("DATE")] public DateTime Date { get; set; }
    [JsonProperty("TYPE")] public int Type { get; set; } = 70;                       // sabit: Kredi kartı ödemesi
    [JsonProperty("NOTES1")] public string? Notes1 { get; set; }
    [JsonProperty("TOTAL_CREDIT")] public decimal TotalCredit { get; set; }
    [JsonProperty("PRINT_COUNTER")] public int PrintCounter { get; set; } = 3;        // sabit
    [JsonProperty("PRINT_DATE")] public DateTime? PrintDate { get; set; }
    [JsonProperty("ACCFICHEREF")] public int? AccFicheref { get; set; }
    [JsonProperty("CURRSEL_TOTALS")] public int CurrSelTotals { get; set; } = 1;      // sabit
    [JsonProperty("TRANSACTIONS")] public KrediKartTahsilatLogoTransactionsDto Transactions { get; set; } = new();
    [JsonProperty("ARP_CODE")] public string? ArpCode { get; set; }
    [JsonProperty("TIME")] public int Time { get; set; }
    [JsonProperty("BANKACC_CODE")] public string? BankAccCode { get; set; }
    [JsonProperty("HOUR")] public int? Hour { get; set; }
    [JsonProperty("MINUTE")] public int? Minute { get; set; }
    [JsonProperty("GUID")] public string? Guid { get; set; }
}

/// <summary>
/// Cari hesap işlemleri wrapper'ı — Logo formatında { "items": [...] }.
/// </summary>
public class KrediKartTahsilatLogoTransactionsDto
{
    [JsonProperty("items")] public List<KrediKartTahsilatLogoTransactionDto> Items { get; set; } = new();
}

/// <summary>
/// Kredi kartı tahsilat işlemi (Logo payload).
/// BANK_GL_CODE gönderilmez; Logo FillAccCodesOnPreSave ile save sonrası otomatik doldurur.
/// </summary>
public class KrediKartTahsilatLogoTransactionDto
{
    [JsonProperty("ARP_CODE")] public string ArpCode { get; set; } = string.Empty;
    [JsonProperty("TRANNO")] public string? Tranno { get; set; }
    [JsonProperty("CREDIT")] public decimal Credit { get; set; }
    [JsonProperty("TC_AMOUNT")] public decimal TcAmount { get; set; }
    [JsonProperty("BNLN_TC_AMOUNT")] public decimal BnlnTcAmount { get; set; }
    [JsonProperty("PAYMENT_LIST")] public KrediKartTahsilatLogoPaymentListDto PaymentList { get; set; } = new();
    [JsonProperty("MONTH")] public int Month { get; set; }
    [JsonProperty("YEAR")] public int Year { get; set; }
    [JsonProperty("AFFECT_RISK")] public int AffectRisk { get; set; } = 1;            // sabit
    [JsonProperty("BANKACC_CODE")] public string? BankAccCode { get; set; }
    [JsonProperty("GUID")] public string? Guid { get; set; }
}

/// <summary>
/// Ödeme listesi wrapper'ı — { "items": [...] }.
/// </summary>
public class KrediKartTahsilatLogoPaymentListDto
{
    [JsonProperty("items")] public List<KrediKartTahsilatLogoPaymentDto> Items { get; set; } = new();
}

/// <summary>
/// Kredi kartı ödemesi kalemi (Logo payload).
/// CARDREF artık payload'a dahil edilmez (null iken serialize olmaz).
/// </summary>
public class KrediKartTahsilatLogoPaymentDto
{
    [JsonProperty("CARDREF", NullValueHandling = NullValueHandling.Ignore)]
    public int? Cardref { get; set; }

    [JsonProperty("DATE")] public DateTime Date { get; set; }
    [JsonProperty("MODULENR")] public int Modulenr { get; set; } = 5;                 // sabit
    [JsonProperty("FICHEREF")] public int? Ficheref { get; set; }
    [JsonProperty("TRCODE")] public int? Trcode { get; set; }
    [JsonProperty("TOTAL")] public decimal Total { get; set; }
    [JsonProperty("PROCDATE")] public DateTime Procdate { get; set; }
    [JsonProperty("BANKACC_CODE")] public string? BankAccCode { get; set; }
    [JsonProperty("PAYMENT_TYPE")] public int PaymentType { get; set; } = 4;          // sabit
    [JsonProperty("DISCTRDELLIST")] public int Disctrdellist { get; set; } = 4;       // sabit
}
