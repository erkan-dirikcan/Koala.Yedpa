namespace Koala.Yedpa.Core.Services
{
    public interface ILicenseValidator
    {
        bool IsLicenseValid();
        string? GetXKey();
        string? GetApplicationId();
    }
}
