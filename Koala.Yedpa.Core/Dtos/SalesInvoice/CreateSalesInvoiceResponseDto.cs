using System.Text.Json.Serialization;

namespace Koala.Yedpa.Core.Dtos.SalesInvoice;

/// <summary>
/// Logo Go satış faturası oluşturma yanıtı DTO'su
/// </summary>
public class CreateSalesInvoiceResponseDto
{
    /// <summary>
    /// Logo'daki fatura referans numarası (LogicalRef)
    /// </summary>
    [JsonPropertyName("LOGICAL_REF")]
    public int InvoiceRef { get; set; }

    /// <summary>
    /// Fatura numarası (Logo tarafından atanan)
    /// </summary>
    [JsonPropertyName("DOC_NUMBER")]
    public string InvoiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// İşlem başarılı mı?
    /// </summary>
    [JsonPropertyName("IS_SUCCESS")]
    public bool IsSuccess { get; set; }

    /// <summary>
    /// İşlem mesajı
    /// </summary>
    [JsonPropertyName("MESSAGE")]
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Logo Go'dan gelen ham yanıt (debugging için)
    /// </summary>
    [JsonPropertyName("RAW_RESPONSE")]
    public string? RawResponse { get; set; }

    /// <summary>
    /// İşlem zamanı
    /// </summary>
    [JsonPropertyName("PROCESSED_AT")]
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Hata varsa hata detayı
    /// </summary>
    [JsonPropertyName("ERROR_DETAIL")]
    public string? ErrorDetail { get; set; }
}
