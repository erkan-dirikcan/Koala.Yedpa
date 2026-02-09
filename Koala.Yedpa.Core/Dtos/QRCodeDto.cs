namespace Koala.Yedpa.Core.Dtos;

public class QRCodeDto
{
    public string Content { get; set; }
    public int PixelSize { get; set; } = 10;
    public string? LogoFilePath { get; set; }
    public bool IncludeLogo { get; set; } = false;
}
