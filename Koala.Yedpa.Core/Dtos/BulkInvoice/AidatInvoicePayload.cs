using System.Text.Json.Serialization;

namespace Koala.Yedpa.Core.Dtos.BulkInvoice
{
    /// <summary>
    /// Logo REST <c>salesInvoices</c> AIDAT fatura payload'ı.
    /// Yapı, repo kökündeki <c>test-aidat-fatura-temmuz.json</c> ile manuel doğrulandı (başarılı POST).
    /// Toplamlar/VAT kırılımı (TOTAL, VAT_AMOUNT, VAT_BASE, TOTAL_NET) GÖNDERİLMEZ — REST hesaplar.
    /// GL kodları DataObjectParameter.FillAccCodesOnPreSave ile otomatik dolar.
    /// </summary>
    public class AidatInvoicePayload
    {
        [JsonPropertyName("GRP_CODE")] public int GrpCode { get; set; } = 2;
        [JsonPropertyName("TYPE")] public int Type { get; set; } = 7;
        [JsonPropertyName("NUMBER")] public string Number { get; set; } = "~"; // Logo otomatik no atar
        [JsonPropertyName("DATE")] public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
        [JsonPropertyName("TIME")] public int Time { get; set; }
        [JsonPropertyName("DOC_NUMBER")] public string DocNumber { get; set; } = "AIDAT";
        [JsonPropertyName("AUTH_CODE")] public string AuthCode { get; set; } = "AIDAT";
        [JsonPropertyName("ARP_CODE")] public string ArpCode { get; set; } = string.Empty;
        [JsonPropertyName("NOTES1")] public string Notes1 { get; set; } = string.Empty;
        [JsonPropertyName("PAYMENT_CODE")] public string PaymentCode { get; set; } = "10-3"; // tüm AIDAT için sabit
        [JsonPropertyName("EINVOICE")] public int EInvoice { get; set; } = 1;
        [JsonPropertyName("PROFILE_ID")] public int ProfileId { get; set; } = 1;
        [JsonPropertyName("EINSTEAD_OF_DISPATCH")] public int EInsteadOfDispatch { get; set; } = 1;
        [JsonPropertyName("EDTCURR_GLOBAL_CODE")] public string EdtCurrGlobalCode { get; set; } = "TL";
        [JsonPropertyName("TRANSACTIONS")] public AidatInvoiceTransactions Transactions { get; set; } = new();
    }

    /// <summary>
    /// Logo, satırları düz dizi değil <c>{ "items": [ ... ] }</c> sarmalı içinde bekler (doğrulandı).
    /// </summary>
    public class AidatInvoiceTransactions
    {
        [JsonPropertyName("items")] public List<AidatInvoiceTransaction> Items { get; set; } = new();
    }

    /// <summary>
    /// AIDAT fatura kalemi. Tek satır: sabit hizmet kartı + ayın tutarı (KDV dahil).
    /// </summary>
    public class AidatInvoiceTransaction
    {
        [JsonPropertyName("TYPE")] public int Type { get; set; } = 4;
        [JsonPropertyName("MASTER_CODE")] public string MasterCode { get; set; } = "600.11.0001"; // sabit AIDAT hizmet kartı
        [JsonPropertyName("QUANTITY")] public int Quantity { get; set; } = 1;
        [JsonPropertyName("UNIT_CONV1")] public string UnitConv1 { get; set; } = "1";
        [JsonPropertyName("UNIT_CONV2")] public string UnitConv2 { get; set; } = "1";
        [JsonPropertyName("PRICE")] public string Price { get; set; } = string.Empty; // KDV dahil tutar (string)
        [JsonPropertyName("VAT_RATE")] public int VatRate { get; set; } = 20;
        [JsonPropertyName("VAT_INCLUDED")] public int VatIncluded { get; set; } = 1;
        [JsonPropertyName("UNIT_CODE")] public string UnitCode { get; set; } = "ADET";
        [JsonPropertyName("DESCRIPTION")] public string Description { get; set; } = string.Empty;
    }
}
