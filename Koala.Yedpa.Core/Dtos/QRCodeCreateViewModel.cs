using System.ComponentModel.DataAnnotations;

namespace Koala.Yedpa.Core.Dtos;

public class QRCodeCreateViewModel
{
    [Required(ErrorMessage = "QR Kod Yılı gereklidir")]
    public string QrCodeYear { get; set; } = DateTime.Now.Year.ToString();

    [Required(ErrorMessage = "QR Kod Ön Kodu gereklidir")]
    [StringLength(50, ErrorMessage = "QR Kod Ön Kodu en fazla 50 karakter olabilir")]
    public string QrCodePreCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "SQL Sorgusu gereklidir")]
    public string SqlQuery { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Açıklama en fazla 500 karakter olabilir")]
    public string Description { get; set; } = string.Empty;
}
