namespace Koala.Yedpa.Core.Dtos;

public class QRCodeDto
{
    public string Text { get; set; } = string.Empty;
    public int Width { get; set; } = 300;
    public int Height { get; set; } = 300;
    public string? LogoFilePath { get; set; }
    public bool IncludeLogo { get; set; } = false;
}
