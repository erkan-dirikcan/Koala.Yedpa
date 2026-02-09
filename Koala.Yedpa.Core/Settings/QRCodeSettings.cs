namespace Koala.Yedpa.Core.Configuration;

public class QRCodeSettings
{
    public const string SectionName = "QRCodeSettings";

    public string QrCodePrefix { get; set; } = "G11522-Yd";
    public string QrCodeStoragePath { get; set; } = "qrcodes";
    public string? LogoFilePath { get; set; }
    public int DefaultPixelSize { get; set; } = 10;
    public string QrCodeBaseUrl { get; set; } = "https://yedpa.sistem-koala.com";
}
